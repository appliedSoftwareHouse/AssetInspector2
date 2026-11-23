using AppModels.DB;
using Microsoft.Data.SqlClient;
using System.Data;
namespace DB.Core
{
    public class CreateDB
    {
        public SqlConnection Connection { get; private set; }
        public string DatabaseName { get; private set; }

        #region Constructor
        public CreateDB(string connectionString, string _DatabaseName)
        {
            Connection = new SqlConnection(connectionString);
            DatabaseName = _DatabaseName;
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
        #endregion

        #region Read SQLite DataTable

        public bool CreateTableFromDataTable()
        {
            try
            {
                // Generate the columns script based on the DataTable schema
                string columnsScript = string.Empty;

                var _SQLiteFile = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "System", "CreateDB.db");
                var _SQLiteService = new SQLiteService($"Source={_SQLiteFile}");

                var _Queries = _SQLiteService.GetCreateTableQueries();
                    

                foreach (var item in _Queries)
                {
                    CreateTable(item.TableName, item.ColumnsScript);
                }

                return true;
            }
            catch (Exception)
            {
                // Optionally log the exception details before returning fals
                return false;
            }
        }


        #endregion


    }
}
