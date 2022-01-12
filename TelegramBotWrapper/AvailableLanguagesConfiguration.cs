namespace Telegram.Bot.Wrapper;

public class AvailableLanguage
{
    public string LanguageName { get; init; } = "";
    public string LanguageCode { get; init; } = "";
}

public class AvailableLanguagesConfiguration
{
    public const string ConfigName = "AvailableLanguages";
    public IEnumerable<AvailableLanguage>? Languages { get; set; }
}