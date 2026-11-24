using AppModels.DB;
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

        #region Constructor
        public CreateDB(string? SQLConnectionString, string? _DatabaseName, string? _SQLiteString)
        {
            if(SQLConnectionString == null || _DatabaseName == null || _SQLiteString == null)
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
            if(!IsConnected()) { return false; }

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


            if (_Queries.Count == 0)
                return false;

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
        public bool CreateTable(string tableName, string columnsScript)
        {
            if (string.IsNullOrWhiteSpace(tableName) ||
                string.IsNullOrWhiteSpace(columnsScript))
            {
                return false;
            }

            try
            {
                if (Connection.State != ConnectionState.Open)
                {
                    Connection.Open();
                }

                using (var command = Connection.CreateCommand())
                {
                    // Escaping ']' prevents syntax breaks if a name contains a closing bracket
                    string safeTable = tableName.Replace("]", "]]");

                    command.CommandText = $@" USE [{DatabaseName}];
                                            IF OBJECT_ID(N'[{safeTable}]', N'U') IS NULL
                                            BEGIN
                                                CREATE TABLE [{safeTable}] ({columnsScript});
                                            END";

                    command.ExecuteNonQuery();
                    return true;
                }
            }
            catch (SqlException)
            {
                // Optionally log the exception details before returning false
                return false;
            }
            finally
            {
                if (Connection.State == ConnectionState.Open)
                {
                    Connection.Close();
                }
            }
        }
        private bool CreateNewDataTable(CreateTableQueryModel query, SqlTransaction transaction)
        {
            try
            {
                var _Text = new StringBuilder();

                _Text.Append($"CREATE TABLE [{query.TableName}] ");
                _Text.Append(query.ColumnsScript);

                using SqlCommand _Command = new(
                    _Text.ToString(),
                    Connection,
                    transaction);

                _Command.ExecuteNonQuery();

                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool UpdatePatches()
        {
            throw new NotImplementedException();
        }

        #endregion




    }
}
