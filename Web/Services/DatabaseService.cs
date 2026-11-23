using AppModels.DB;
using DB.Core;
using Microsoft.Data.SqlClient;
using System.Text;

namespace Web.Services
{
    public interface IDatabaseService
    {
        bool IsConnected { get; }
        bool IsDBExist { get; }
        bool CreateDataTables();
    }

    public class DatabaseService : IDatabaseService
    {
        public IConfiguration Config { get; set; }
        public CreateDB CreateDBService { get; set; }
        public SQLiteService LiteService { get; set; }

        public DatabaseService(IConfiguration _config)
        {
            Config = _config;

            string connectionString = Config.GetConnectionString("SQLConnection") ?? "";
            string SQLiteString = Config.GetConnectionString("SQLiteCreateDB") ?? "";

            string DatabaseName = Config["DatabaseName"] ?? "";

            CreateDBService = new CreateDB(connectionString, DatabaseName);
            LiteService = new(SQLiteString);
        }


        public bool IsConnected => CreateDBService.IsConnected();
        public bool IsDBExist => CreateDBService.CreateDatabase(); 
        public bool CreateDataTables()
        {
            var _Queries = LiteService.GetCreateTableQueries();
            var IsCreated = false;


            if (_Queries.Count == 0)
                return false;

            using var transaction = CreateDBService.Connection.BeginTransaction();

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
            try
            {
                var _Text = new StringBuilder();

                _Text.Append($"CREATE TABLE [{query.TableName}] ");
                _Text.Append(query.ColumnsScript);

                using SqlCommand _Command = new(
                    _Text.ToString(),
                    CreateDBService.Connection,
                    transaction);

                _Command.ExecuteNonQuery();

                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
