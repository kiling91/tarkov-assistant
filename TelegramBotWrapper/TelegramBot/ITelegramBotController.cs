using Telegram.Bot.Wrapper.UserRegistry;

namespace Telegram.Bot.Wrapper.TelegramBot;

public interface ITelegramBotController
{
    MenuItem InitMainMenu();
    Task OnInlineMenuCallBack(string key, UserProfile user, string? data);
}