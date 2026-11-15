namespace LanguagePack;

public record LanguageInfo(string Code, string Name);

public static class Languages
{
    public static readonly IReadOnlyList<LanguageInfo> All = new List<LanguageInfo>
    {
        new("en", "English"),
        new("ar", "العربية")
    };

    public static string[] Codes => All.Select(l => l.Code).ToArray();
}