using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using System.Data;

namespace AppConfig
{
    public class ConfigModel
    {
        public IConfiguration Config { get; set; }
        private SqliteConnection? Connection { get; set; }

        private string QueryKey = "SELECT [Value] FROM  [ConfigKeys] WHERE [Key] = @Key";
        
        public ConfigModel(IConfiguration _config)
        {
            Config = _config;
            string connectionString = Config.GetConnectionString("SQLiteConfigDB") ?? "";
            if(!string.IsNullOrWhiteSpace(connectionString))
            {

                string dbPath = Path.Combine(Directory.GetCurrentDirectory(), connectionString);
                Connection = new SqliteConnection($"Data Source={dbPath}");
            }

        }

        public bool IsConnected()
        {
            if(Connection == null) { return false; }

            try
            {
                if (Connection.State != ConnectionState.Open) { Connection.Open(); }
                return Connection.State == ConnectionState.Open;
            }
            catch (Exception)
            {

                return false;
            }
        }

        public T GetValue<T>(string Key)
        {
            if (Connection == null)
                return default!;

            try
            {
                if (Connection.State != ConnectionState.Open)
                    Connection.Open();

                using var cmd = Connection.CreateCommand();
                cmd.CommandText = QueryKey;
                cmd.Parameters.AddWithValue("@Key", Key);

                var result = cmd.ExecuteScalar();

                if (result == null || result == DBNull.Value)
                    return default!;

                Type targetType = Nullable.GetUnderlyingType(typeof(T)) ?? typeof(T);

                return (T)Convert.ChangeType(result, targetType);
            }
            catch
            {
                return default!;
            }

        }

        public async Task<T> GetValueAsync<T>(string Key)
        {
            if (Connection == null)
                return default!;

            try
            {
                if (Connection.State != ConnectionState.Open)
                    await Connection.OpenAsync();

                using var cmd = Connection.CreateCommand();
                cmd.CommandText = QueryKey;
                cmd.Parameters.AddWithValue("@Key", Key);

                var result = await cmd.ExecuteScalarAsync();

                if (result == null || result == DBNull.Value)
                    return default!;

                Type targetType = Nullable.GetUnderlyingType(typeof(T)) ?? typeof(T);

                return (T)Convert.ChangeType(result, targetType);
            }
            catch
            {
                return default!;
            }
        }

        public class ConfigVM
        {
            public long ID { get; set; }
            public string Key { get; set; } = string.Empty;
            public string Value { get; set; } = string.Empty;
            public string KeyType { get; set; } = string.Empty;
            public string Remarks { get; set; } = string.Empty;
        }

    }
}
