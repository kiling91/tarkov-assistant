using Telegram.Bot.Wrapper.UserRegistry;

namespace Tarkov.Assistant.Telegram.Bot.Feature;

public class UserRegistry: IUserRegistry
{
    public async Task<UserProfile> FindOrCreateUser(long userId, string firstName, string? lastName, string? lang)
    {
        await Task.CompletedTask;
        return new UserProfile()
        {
            Id = userId,
        };
    }

    public Task<UserProfile?> FindUser(long userId)
    {
        throw new NotImplementedException();
    }
}