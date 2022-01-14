namespace Telegram.Bot.Wrapper.UserRegistry;

public struct InlineMenuData
{
    public bool Exist { get; set; } = false;
    public string? Key { get; set; }
    public string? Data { get; set; }
}

public interface IUserStateManager
{
    Task SetActualMenuName(long userId, string menuName);
    Task<string> GetActualMenuName(long userId);
    Task SetInlineMenuData(long userId, string key, string uid, string? data);
    Task<InlineMenuData> GetInlineMenuData(long userId, string uid);
    Task RemoveInlineMenuData(long userId, string key);
}