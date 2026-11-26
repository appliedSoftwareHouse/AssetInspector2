using DB.Core;
using Microsoft.Data.SqlClient;

namespace Web.Services
{
    public interface IDatabaseService
    {
        bool IsConnected { get; }
        bool IsDBExist { get; }
        bool CreateDataTables { get; }
        bool UpdatePatches { get; }
        bool CreateAdminAccount { get; }
        DBContextClass DBContext { get; set; }
        SqlConnection Connection => DBContext.Connection;
    }

    public class DatabaseService : IDatabaseService
    {
        public CreateDB CreateDBService { get; set; }
        public DBContextClass DBContext { get; set; }


        public DatabaseService(IConfigurationService Config)
        {
            var _SQLConnectionString = Config.SQLConnectionString();
            var _DataBaseName = Config.GetValue<string>("DataBaseName");
            var _SQLiteString = Config.GetValue<string>("SQLiteCreateDB");

            if( _SQLConnectionString != null)
            {
                CreateDBService = new(_SQLConnectionString, _DataBaseName, _SQLiteString);
                DBContext = new(_SQLConnectionString);
            }
            else
            {
                CreateDBService = null!;
                DBContext = null!;
            }
        }

        public bool IsConnected => CreateDBService.IsConnected();
        public bool IsDBExist => CreateDBService.CreateDatabase();
        public bool CreateDataTables => CreateDBService.CreateDataTables();
        public bool UpdatePatches => CreateDBService.UpdatePatches();
        public bool CreateAdminAccount => CreateDBService.CreateAdminUser();
    }
}

