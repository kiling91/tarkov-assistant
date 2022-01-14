using Telegram.Bot.Wrapper.UserRegistry;

namespace Telegram.Bot.Wrapper.TelegramBot;

public interface ITelegramBotController
{
    MenuItem InitMainMenu();
    Task<bool> OnInlineMenuCallBack(UserProfile user, string key, string? data);
    Task<bool> OnUserInputCallBack(UserProfile user, string message);

    Task OnBeforeCallBack(UserProfile user);
}