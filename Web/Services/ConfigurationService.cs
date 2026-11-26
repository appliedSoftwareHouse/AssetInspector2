using AppConfig;

namespace Web.Services
{
    public interface IConfigurationService
    {
        bool IsConnected { get; }

        
        T GetValue<T>(string Key);
        Task<T> GetValueAsync<T>(string Key);
        string? SQLConnectionString();
        string? SQLiteConnectionString();
        T GetConfigValue<T>(string Configkey);
        ConfigModel MyModel { get; set; }
    }


    public class ConfigurationService : IConfigurationService
    {
        public ConfigModel MyModel { get; set; }
        public IConfiguration Config { get; set; }

        public ConfigurationService(IConfiguration _config)
        {
            Config = _config;
            MyModel = new(Config);
        }

        public bool IsConnected => MyModel.IsConnected();
        public T GetValue<T>(string Key) => MyModel.GetValue<T>(Key);
        public async Task<T> GetValueAsync<T>(string Key) => await MyModel.GetValueAsync<T>(Key);
        public string? SQLConnectionString()
        {
            return Config.GetConnectionString("SQLConnection");
        }

        public T GetConfigValue<T>(string Configkey)
        {
            return Config.GetValue<T>("ConfigKey") ?? default! ;
        }

        public string? SQLiteConnectionString()
        {
            return MyModel.GetSQLiteConnectionString();
        }
    }
}
