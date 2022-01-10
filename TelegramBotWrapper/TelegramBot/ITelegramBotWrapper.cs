using Telegram.Bot.Wrapper.UserRegistry;

namespace Telegram.Bot.Wrapper.TelegramBot;

public interface ITelegramBotWrapper
{
    void SetupMainMenu(MenuItem mainMenu);
    Task<bool> DrawMenu(string? text, UserProfile user);
    Task DrawMainMenu(UserProfile user);
    Task SendText(UserProfile user, string text);
    Task SendMenu(UserProfile user, string text, MenuItem menu);
    Task SendInlineMenu(UserProfile user, string text, InlineMenu inlineMenu);
}