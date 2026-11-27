
using Microsoft.Data.Sqlite;

namespace AppConfig
{
    public class KeysStore
    {
        private readonly string ConnectionString;

        private volatile Dictionary<string, ConfigKey> Configkeys = [];

        public record ConfigKey(string Value, string KeyType);

        public KeysStore(string dbPath)
        {
            string? directory = Path.GetDirectoryName(dbPath);

            if (!string.IsNullOrEmpty(directory))
                Directory.CreateDirectory(directory);

            ConnectionString = new SqliteConnectionStringBuilder
            {
                DataSource = dbPath
            }.ToString();

            ReLoad();
        }

        private void ReLoad()
        {
            var keys = new Dictionary<string, ConfigKey>(
                StringComparer.OrdinalIgnoreCase);

            using var conn = new SqliteConnection(ConnectionString);
            conn.Open();

            using var cmd = conn.CreateCommand();

            cmd.CommandText = "SELECT [Key], [Value], [KeyType] FROM [ConfigKeys] ORDER BY [Key]";

            using var read = cmd.ExecuteReader();

            while (read.Read())
            {
                string key = read.GetString(0);
                string value = read.GetString(1);
                string keyType = read.GetString(2);

                keys[key] = new ConfigKey(value, keyType);
            }

            Configkeys = keys;
        }

        public T Get<T>(string key, T defaultValue = default!)
        {
            if (!Configkeys.TryGetValue(key, out var item))
                return defaultValue;

            try
            {
                Type targetType = typeof(T);

                if (targetType == typeof(string))
                    return (T)(object)item.Value;

                if (targetType == typeof(bool))
                    return (T)(object)bool.Parse(item.Value);

                Type underlyingType =
                    Nullable.GetUnderlyingType(targetType) ?? targetType;

                object result = Convert.ChangeType(
                    item.Value,
                    underlyingType,
                    System.Globalization.CultureInfo.InvariantCulture);

                return (T)result;
            }
            catch
            {
                return defaultValue;
            }
        }

        public void Reload()
        {
            ReLoad();
        }
    }
}
