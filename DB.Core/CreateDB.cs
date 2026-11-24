using AppModels.DB;
using Crypto;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Text;

namespace DB.Core
{
    public class CreateDB
    {
        public SqlConnection Connection { get; private set; }
        public string DatabaseName { get; private set; }
        public SQLiteClass LiteService { get; set; }
        private string EncryptKey = "Modern";

        #region Constructor
        public CreateDB(string? SQLConnectionString, string? _DatabaseName, string? _SQLiteString)
        {
            if (SQLConnectionString == null || _DatabaseName == null || _SQLiteString == null)
            {
                DatabaseName = null!;
                Connection = null!;
                LiteService = null!;
            }
            else
            {
                DatabaseName = _DatabaseName;
                Connection = new SqlConnection(SQLConnectionString);
                LiteService = new(_SQLiteString);
            }
        }


        #endregion

        #region Core Method
        public bool CreateDatabase()
        {
            if (!IsConnected()) { return false; }

            try
            {
                if (Connection.State != ConnectionState.Open)
                    Connection.Open();

                using var command = Connection.CreateCommand();
                command.CommandText = @"
                                    IF DB_ID(@name) IS NULL
                                    BEGIN
                                        DECLARE @sql NVARCHAR(MAX) = N'CREATE DATABASE ' + QUOTENAME(@name);
                                        EXEC sp_executesql @sql;
                                    END";

                command.Parameters.Add(new SqlParameter("@name", SqlDbType.NVarChar, 128) { Value = DatabaseName });
                command.ExecuteNonQuery();

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        public bool IsConnected()
        {
            try
            {
                if (Connection.State != ConnectionState.Open)
                    Connection.Open();

                return Connection.State == ConnectionState.Open;
            }
            catch (Exception)
            {
                return false;
            }
        }


        public bool CreateDataTables()
        {
            var _Queries = LiteService.GetCreateTableQueries();
            var IsCreated = false;


            if (_Queries.Count == 0) { return IsCreated; }

            using var transaction = Connection.BeginTransaction();

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

                using SqlCommand command = new(sql.ToString(), Connection, transaction);

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
            try
            {
                string sql = $"SELECT CASE WHEN EXISTS (SELECT 1 FROM [{DatabaseName}].sys.tables WHERE name = @TableName ) THEN 1 ELSE 0 END;";

                using SqlCommand command = new(sql, Connection, transaction);

                command.Parameters.AddWithValue("@TableName", tableName);

                return Convert.ToBoolean(command.ExecuteScalar());
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

                _Transaction = Connection.BeginTransaction();

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

                using (SqlCommand _Command = new(_SQL, Connection, _Transaction))
                {
                    _Command.Parameters.AddWithValue("@IdHash", Guid.NewGuid().ToString());
                    _Command.Parameters.AddWithValue("@UserName", _Admin);
                    _Command.Parameters.AddWithValue("@PasswordHash", _PasswordHash);
                    _Command.Parameters.AddWithValue("@SecurityStamp", Guid.NewGuid().ToString());
                    _Command.Parameters.AddWithValue("@FullName", "System Administrator");
                    _Command.Parameters.AddWithValue("@Email", "admin@localhost.com");
                    _Command.Parameters.AddWithValue("@TransactionID", _TransGuid.Value);

                    _Command.ExecuteNonQuery();
                }

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
                using SqlCommand _Command = new(_SQL, Connection, transaction);

                _Command.Parameters.AddWithValue("@TransactionID", _TransactionID);
                _Command.Parameters.AddWithValue("@TransactionName", TransactionName);
                _Command.Parameters.AddWithValue("@TransactionByUserID", TransactionByUserID);
                _Command.Parameters.AddWithValue("@TransactionType", TransactionType);

                _Command.ExecuteNonQuery();

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
