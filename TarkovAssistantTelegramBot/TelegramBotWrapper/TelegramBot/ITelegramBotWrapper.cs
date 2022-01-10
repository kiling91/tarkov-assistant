using TelegramBotWrapper.UserRegistry;

namespace TelegramBotWrapper.TelegramBotWrapper;

public interface ITelegramBotWrapper
{
    void SetupMainMenu(MenuItem mainMenu);
    Task<bool> DrawMenu(string? text, UserProfile user);
    Task DrawMainMenu(UserProfile user);

    Task Send(UserProfile user, string text, MenuItem? menu = null);
}