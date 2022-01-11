namespace Telegram.Bot.Wrapper;

public class BotConfiguration
{
    public const string ConfigName = "BotConfiguration";
    public string BotToken { get; init; } = "";
    public string HostAddress { get; init; } = "";
}
