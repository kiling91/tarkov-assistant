namespace Telegram.Bot.Wrapper.UserRegistry;

public class UserProfile
{
    public long Id { get; init; }
    public string? Lang { get; init; }
    public string FirstName { get; init; } = "unknown";
    public string? LastName { get; init; }
}