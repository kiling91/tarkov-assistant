namespace TelegramBotWrapper.UserRegistry;

public interface IUserRegistry
{
    Task<UserProfile> FindOrCreateUser(long userId, string firstName, string? lastName, string? lang);
}