using AppConfig;
using AppModels.DB;
using Crypto;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Text;

namespace DB.Core
{
    public class CreateDB
    {
        public DBContextClass db { get; set; }
        public ConfigKeys Keys { get; set; }
        SQLiteClass LiteService { get; set; }
        private string EncryptKey = "Modern";

        private string DatabaseName { get; set; }

        #region Constructor
        public CreateDB(DBContextClass _db, ConfigKeys _Keys)
        {
            db = _db;
            Keys = _Keys;

            DatabaseName = Keys.Get<string>("DBName");
            LiteService = new(Keys.Get<string>("DBCreateQuriesFile"));
        }


        #endregion

        #region Core Method
        public bool CreateDatabase()
        {
            if (!db.IsConnected) { return false; }

            try
            {
              
                db.Command.CommandText = @"
                                    IF DB_ID(@name) IS NULL
                                    BEGIN
                                        DECLARE @sql NVARCHAR(MAX) = N'CREATE DATABASE ' + QUOTENAME(@name);
                                        EXEC sp_executesql @sql;
                                    END";

                db.Command.Parameters.Add(new SqlParameter("@name", SqlDbType.NVarChar, 128) { Value = DatabaseName });
                db.Command.ExecuteNonQuery();

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        public bool IsConnected => db.IsConnected;


        public bool CreateDataTables()
        {
            var _Queries = LiteService.GetCreateTableQueries();
            var IsCreated = false;


            if (_Queries.Count == 0) { return IsCreated; }

            using var transaction = db.Connection.BeginTransaction();

            try
            {
                foreach (CreateTableQueryModel query in _Queries)
                {
                    IsCreated = CreateNewDataTable(query, transaction);
                    if (!IsCreated)
                    {
                        transaction.Rollback();
                        return false;
                    }
                }

                transaction.Commit();
                return true;
            }
            catch
            {
                transaction.Rollback();
                return false;
            }
        }

        private bool CreateNewDataTable(CreateTableQueryModel query, SqlTransaction transaction)
        {
            if (query == null ||
            string.IsNullOrWhiteSpace(query.TableName) ||
            string.IsNullOrWhiteSpace(query.ColumnsScript))
            {
                return false;
            }



            try
            {
                if (IsTableExists(query.TableName, transaction))
                {
                    return true;
                }

                var sql = new StringBuilder();

                sql.Append($"CREATE TABLE [{DatabaseName}].[dbo].[{query.TableName}] ");
                sql.Append(query.ColumnsScript);

                using SqlCommand command = new(sql.ToString(), db.Connection, transaction);

                command.ExecuteNonQuery();

                return true;
            }
            catch
            {
                return false;
            }
        }

        private bool IsTableExists(string tableName, SqlTransaction transaction = null!)
        {
            if(DatabaseName == null) { return false; }

            try
            {
                string _SQL = $"SELECT CASE WHEN EXISTS (SELECT 1 FROM [{DatabaseName}].sys.tables WHERE name = @TableName ) THEN 1 ELSE 0 END;";

                db.Command.CommandText = _SQL;
                db.Command.Parameters.Clear();
                db.Command.Parameters.AddWithValue("@TableName", tableName);

                return Convert.ToBoolean(db.Command.ExecuteScalar());
            }
            catch
            {
                return false;
            }
        }

        public bool CreateAdminUser()
        {
            SqlTransaction _Transaction = null!;

            try
            {
                var _Admin = "Admin";
                var _Password = "123456";
                var _PasswordHash = Encrypt.EncryptString(_Password, EncryptKey);

                _Transaction = db.Connection.BeginTransaction();

                // 1. Create System Transaction
                var _TransGuid = CreateSystemTransaction(
                    "Created Administrator Account",
                    0,
                    1,
                    _Transaction);

                if (_TransGuid == null)
                {
                    _Transaction.Rollback();
                    return false;
                }

                // 2. Create Admin User
                var _SQL = $@"
                            IF NOT EXISTS
                            (
                                SELECT 1
                                FROM [{DatabaseName}].[dbo].[Users]
                                WHERE [UserName] = @UserName
                            )
                            BEGIN
                                INSERT INTO [Web2].[dbo].[Users]
                                (
                                    [IdHash],
                                    [UserName],
                                    [PasswordHash],
                                    [SecurityStamp],
                                    [FullName],
                                    [Email],
                                    [EmailConfirmed],
                                    [PhoneNumber],
                                    [PhoneNumberConfirmed],
                                    [IsActive],
                                    [MustChangePasswordNextLogin],
                                    [DefaultLanguageID],
                                    [IsADAccount],
                                    [AccessFailedCount],
                                    [LockoutEnabled],
                                    [TwoFactorEnabled],
                                    [AddTransactionID]
                                )
                                VALUES
                                (
                                    @IdHash,
                                    @UserName,
                                    @PasswordHash,
                                    @SecurityStamp,
                                    @FullName,
                                    @Email,
                                    0,
                                    '',
                                    0,
                                    1,
                                    1,
                                    1,
                                    0,
                                    0,
                                    0,
                                    0,
                                    @TransactionID
                                );
                            END";

                db.Command.CommandText = _SQL;
                db.Command.Parameters.Clear();


                    db.Command.Parameters.AddWithValue("@IdHash", Guid.NewGuid().ToString());
                    db.Command.Parameters.AddWithValue("@UserName", _Admin);
                    db.Command.Parameters.AddWithValue("@PasswordHash", _PasswordHash);
                    db.Command.Parameters.AddWithValue("@SecurityStamp", Guid.NewGuid().ToString());
                    db.Command.Parameters.AddWithValue("@FullName", "System Administrator");
                    db.Command.Parameters.AddWithValue("@Email", "admin@localhost.com");
                    db.Command.Parameters.AddWithValue("@TransactionID", _TransGuid.Value);

                    db.Command.ExecuteNonQuery();
                

                // 3. Commit
                _Transaction.Commit();

                return true;
            }
            catch
            {
                if (_Transaction?.Connection != null)
                    _Transaction.Rollback();

                return false;
            }
            finally
            {
                _Transaction?.Dispose();
            }
        }

        public Guid? CreateSystemTransaction(
                    string TransactionName,
                    int TransactionByUserID,
                    int TransactionType,
                    SqlTransaction transaction)
        {
            try
            {
                var _TransactionID = Guid.NewGuid();

                var _SQL = $@"
                            INSERT INTO [{DatabaseName}].[dbo].[SystemTransaction]
                            (
                                [TransactionID],
                                [TransactionName],
                                [TransactionDate],
                                [TransactionByUserID],
                                [TransactionType]
                            )
                            VALUES
                            (
                                @TransactionID,
                                @TransactionName,
                                GETDATE(),
                                @TransactionByUserID,
                                @TransactionType
                            );";

                db.Command.CommandText = _SQL;
                db.Command.Parameters.Clear();

                db.Command.Parameters.AddWithValue("@TransactionID", _TransactionID);
                db.Command.Parameters.AddWithValue("@TransactionName", TransactionName);
                db.Command.Parameters.AddWithValue("@TransactionByUserID", TransactionByUserID);
                db.Command.Parameters.AddWithValue("@TransactionType", TransactionType);

                db.Command.ExecuteNonQuery();

                return _TransactionID;
            }
            catch
            {
                return null;
            }
        }

        public bool UpdatePatches()
        {
            return true;
        }

        #endregion




    }
}
