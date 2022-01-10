using Telegram.Bot.Wrapper.UserRegistry;

namespace Telegram.Bot.Wrapper.TelegramBot;

public interface ICallbackStorage
{
    void AddCallBack(string uid, InlineMenuItem menuItem, Action<InlineMenuItem, UserProfile> callback);
    Task<bool> Invoke(string uid, long userId);
}