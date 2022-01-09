using Tarkov.Assistant.Telegram.Bot.UserRegistry;

namespace Tarkov.Assistant.Telegram.Bot.TelegramBotWrapper;

public interface ITelegramBotWrapper
{
    void SetupMainMenu(MenuItem mainMenu);
    Task<bool> DrawMenu(string? text, UserProfile user);
    Task DrawMainMenu(UserProfile user);

    Task Send(UserProfile user, string text, MenuItem? menu = null);
}