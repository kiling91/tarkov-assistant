using Telegram.Bot.Wrapper.UserRegistry;

namespace Tarkov.Assistant.Telegram.Bot.Feature;

public class UserRegistry: IUserRegistry
{
    private readonly Dictionary<long, UserProfile> _profiles = new();
    public async Task<UserProfile> FindOrCreateUser(long userId, string firstName, string? lastName, string? lang)
    {
        await Task.CompletedTask;
        if (_profiles.ContainsKey(userId))
        {
            return _profiles[userId];
        }
        
        var profile = new UserProfile()
        {
            Id = userId,
            Lang = lang,
            FirstName = firstName,
            LastName = lastName,
        };
        _profiles.Add(userId, profile);
        return profile;
    }

    public async Task<UserProfile?> FindUser(long userId)
    {
        await Task.CompletedTask;
        if (_profiles.ContainsKey(userId))
        {
            return _profiles[userId];
        }

        return null;
    }

    public async Task ChangeLang(long userId, string? lang)
    {
        await Task.CompletedTask;
        var user = await FindUser(userId);
        if (user == null)
            // Заменить на свой класс Exeption
            throw new NullReferenceException($"User with id {userId} not found");
        _profiles[userId] = new UserProfile
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Lang = lang
        };
    }
}