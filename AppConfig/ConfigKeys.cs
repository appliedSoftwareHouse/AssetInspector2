
namespace AppConfig
{
    public class ConfigKeys
    {
        private readonly KeysStore _store;

        public ConfigKeys(KeysStore store)
        {
            _store = store;
        }

        public T Get<T>(string key, T defaultValue = default!)
        => _store.Get(key, defaultValue);

        public void Reload() => _store.Reload();
    }
}
