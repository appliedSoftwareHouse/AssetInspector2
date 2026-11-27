using System.Globalization;

namespace LanguagePack;

public class Translator
{
    private readonly LanguageStore _store;
    public Translator(LanguageStore store) => _store = store;
    public string this[string key] => _store.Get(key, CultureInfo.CurrentUICulture.TwoLetterISOLanguageName);
}