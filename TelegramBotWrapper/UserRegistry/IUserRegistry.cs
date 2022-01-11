namespace Telegram.Bot.Wrapper.UserRegistry;

public interface IUserRegistry
{
    Task<UserProfile> FindOrCreateUser(long userId, string firstName, string? lastName, string? lang);
    Task<UserProfile?> FindUser(long userId);
    Task ChangeLang(long userId, string? lang);
}