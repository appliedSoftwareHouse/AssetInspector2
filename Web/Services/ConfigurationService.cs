using Microsoft.Data.Sqlite;
using AppConfig;

namespace Web.Services
{
    public interface IConfigurationService
    {
        bool IsConnected { get; }

        T GetValue<T>(string Key);
        Task<T> GetValueAsync<T>(string Key);
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
        

    }
}
