using DB.Core;

namespace Web.Services
{
    public interface IDatabaseService
    {
        bool IsConnected { get; }
        bool IsDBExist { get; }
        bool CreateDataTables { get; }
        bool UpdatePatches { get; }
    }

    public class DatabaseService : IDatabaseService
    {
        public CreateDB CreateDBService { get; set; }


        public DatabaseService(IConfigurationService Config)
        {
            var _SQLConnectionString = Config.SQLConnectionString();
            var _DataBaseName = Config.GetValue<string>("DataBaseName");
            var _SQLiteString = Config.GetValue<string>("SQLiteCreateDB");

            CreateDBService = new(_SQLConnectionString, _DataBaseName, _SQLiteString);

        }

        public bool IsConnected => CreateDBService.IsConnected();
        public bool IsDBExist => CreateDBService.CreateDatabase();
        public bool CreateDataTables => CreateDBService.CreateDataTables();
        public bool UpdatePatches => CreateDBService.UpdatePatches();
    }
}

