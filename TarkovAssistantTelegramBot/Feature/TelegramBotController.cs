using Microsoft.Extensions.Localization;
using Telegram.Bot.Wrapper.TelegramBot;
using Telegram.Bot.Wrapper.UserRegistry;

namespace Tarkov.Assistant.Telegram.Bot.Feature;

public class TelegramBotController : ITelegramBotController
{
    private readonly ITelegramBotWrapper _tg;
    private readonly IStringLocalizer<TelegramBotController> _localizer;
    private readonly ILogger<TelegramBotController> _logger;

    public TelegramBotController(ITelegramBotWrapper telegramBot,
        ILogger<TelegramBotController> logger,
        IStringLocalizer<TelegramBotController> localizer)
    {
        _tg = telegramBot;
        _logger = logger;
        _localizer = localizer;
    }

    private void HandlerMenuDefault(MenuItem menu, UserProfile user)
    {
        _logger.LogInformation($"User: {user.Id}, Menu: {menu.Name}");
        _tg.Send(user, $"User: {user.Id}, Menu: {menu.Name}", menu);
    }

    private void HandlerMenuLanguage(MenuItem menu, UserProfile user)
    {
        _logger.LogInformation($"User: {user.Id}, Language");
        _tg.Send(user, $"User: {user.Id}, Language");
    }

    private void HandlerMenuHelp(MenuItem menu, UserProfile user)
    {
        _logger.LogInformation($"User: {user.Id}, Help");
        _tg.Send(user, $"User: {user.Id}, Help");
    }
    
    private void HandlerMenuPersonalAccount(MenuItem menu, UserProfile user)
    {
        _logger.LogInformation($"User: {user.Id}, Personal account");
        _tg.Send(user, $"User: {user.Id}, Personal account");
    }

    public MenuItem InitMainMenu()
    {
        var mainMenu = new MenuItem("MainMenu", null)
        {
            HandlerCallback = HandlerMenuDefault,
        };
        
        mainMenu.AddItem(_localizer["Share contact"], HandlerMenuDefault, MenuItemType.IsRequestPhoneButton);
        mainMenu.AddItem(_localizer["Personal account"], HandlerMenuPersonalAccount, MenuItemType.Text, true);

        mainMenu.AddItem(_localizer["Language"], HandlerMenuLanguage);
        mainMenu.AddItem(_localizer["Help"], HandlerMenuHelp, MenuItemType.Text, true);
        
        return mainMenu;
    }
}