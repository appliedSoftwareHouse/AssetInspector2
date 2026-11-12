using Microsoft.Data.Sqlite;

namespace LanguagePack;

//public record LanguageInfo(string Code, string Name);

public class LanguageStore
{
    private readonly string _connStr;
    private volatile Dictionary<string, Dictionary<string, string>> _words = new();
    private volatile List<LanguageInfo> _languages = new();

    public LanguageStore(string dbPath)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(dbPath)!);
        _connStr = new SqliteConnectionStringBuilder { DataSource = dbPath }.ToString();

        CreateAndSeed();
        Reload();
    }

    public IReadOnlyList<LanguageInfo> Languages => _languages;
    public string[] Codes => _languages.Select(l => l.Code).ToArray();

    // Current language -> default language (first one) -> the key itself
    public string Get(string key, string langCode)
    {
        if (_words.TryGetValue(langCode, out var dict) && dict.TryGetValue(key, out var value))
            return value;

        var defaultCode = _languages.FirstOrDefault()?.Code;
        if (defaultCode != null
            && _words.TryGetValue(defaultCode, out var def)
            && def.TryGetValue(key, out var defValue))
            return defValue;

        return key;
    }

    // Call this after you edit the database
    public void Reload()
    {
        var languages = new List<LanguageInfo>();
        var words = new Dictionary<string, Dictionary<string, string>>(StringComparer.OrdinalIgnoreCase);

        using var conn = new SqliteConnection(_connStr);
        conn.Open();

        using (var cmd = conn.CreateCommand())
        {
            cmd.CommandText = "SELECT Code, Name FROM Languages ORDER BY SortOrder, Code";
            using var r = cmd.ExecuteReader();
            while (r.Read())
                languages.Add(new LanguageInfo(r.GetString(0), r.GetString(1)));
        }

        using (var cmd = conn.CreateCommand())
        {
            cmd.CommandText = "SELECT LangCode, WordKey, WordValue FROM Translations";
            using var r = cmd.ExecuteReader();
            while (r.Read())
            {
                var lang = r.GetString(0);
                if (!words.TryGetValue(lang, out var dict))
                    words[lang] = dict = new Dictionary<string, string>(StringComparer.Ordinal);
                dict[r.GetString(1)] = r.GetString(2);
            }
        }

        _languages = languages;
        _words = words;
    }

    private void CreateAndSeed()
    {
        using var conn = new SqliteConnection(_connStr);
        conn.Open();

        using (var cmd = conn.CreateCommand())
        {
            cmd.CommandText = """
                CREATE TABLE IF NOT EXISTS Languages (
                    Code      TEXT PRIMARY KEY,
                    Name      TEXT NOT NULL,
                    SortOrder INTEGER NOT NULL DEFAULT 0
                );
                CREATE TABLE IF NOT EXISTS Translations (
                    LangCode   TEXT NOT NULL,
                    WordKey    TEXT NOT NULL,
                    WordValue  TEXT NOT NULL,
                    PRIMARY KEY (LangCode, WordKey),
                    FOREIGN KEY (LangCode) REFERENCES Languages(Code)
                );
                """;
            cmd.ExecuteNonQuery();
        }

        // Seed only when the database is brand new
        using (var check = conn.CreateCommand())
        {
            check.CommandText = "SELECT COUNT(*) FROM Languages";
            if ((long)check.ExecuteScalar()! > 0) return;
        }

        using var tx = conn.BeginTransaction();
        using var seed = conn.CreateCommand();
        seed.Transaction = tx;
        seed.CommandText = """
            INSERT INTO Languages (Code, Name, SortOrder) VALUES
                ('en', 'English', 1),
                ('ar', 'العربية', 2);

            INSERT INTO Translations (LangCode, WordKey, WordValue) VALUES
                ('en', 'Home',     'Home'),
                ('en', 'Login',    'Login'),
                ('en', 'Vouchers', 'Vouchers'),
                ('en', 'Reports',  'Reports'),
                ('ar', 'Home',     'الرئيسية'),
                ('ar', 'Login',    'تسجيل الدخول'),
                ('ar', 'Vouchers', 'القسائم'),
                ('ar', 'Reports',  'التقارير');
            """;
        seed.ExecuteNonQuery();
        tx.Commit();
    }
}