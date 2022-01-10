using Microsoft.Extensions.Localization;
using Telegram.Bot.Wrapper.TelegramBot;
using Telegram.Bot.Wrapper.UserRegistry;

namespace Tarkov.Assistant.Telegram.Bot.Feature;

public class TelegramBotController: ITelegramBotController
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
    
    public MenuItem InitMainMenu()
    {
        var mainMenu = new MenuItem("MainMenu", null)
        {
            UploadHandlerCallback = HandlerMenuDefault,
        };

        var languageMenu = new MenuItem(_localizer["Language"], mainMenu)
        {
            UploadHandlerCallback = HandlerMenuLanguage
        };
        
        var helpMenu = new MenuItem(_localizer["Help"], mainMenu)
        {
            UploadHandlerCallback = HandlerMenuHelp
        };
        
        mainMenu.Children.Add(languageMenu);
        mainMenu.Children.Add(helpMenu);
        
        return mainMenu;
    }
}