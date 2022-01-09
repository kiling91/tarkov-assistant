using Tarkov.Assistant.Telegram.Bot.TelegramBotWrapper;
using Tarkov.Assistant.Telegram.Bot.UserRegistry;

namespace Tarkov.Assistant.Telegram.Bot;

public class TelegramBotController: ITelegramBotController
{
    private readonly ITelegramBotWrapper _telegramBot;
    private readonly ILogger<TelegramBotController> _logger;

    public TelegramBotController(ITelegramBotWrapper telegramBot, 
        ILogger<TelegramBotController> logger)
    {
        _telegramBot = telegramBot;
        _logger = logger;
    }

    private void HandlerMenuDefault(MenuItem menu, UserProfile user)
    {
        _logger.LogInformation($"User: {user.Id}, Menu: {menu.Name}");
        _telegramBot.Send(user, "ошибка", menu);
    }
    
    private void HandlerMenuLanguage(MenuItem menu, UserProfile user)
    {
        _logger.LogInformation($"User: {user.Id}, Language");
    }
    
    private void HandlerMenuHelp(MenuItem menu, UserProfile user)
    {
        _logger.LogInformation($"User: {user.Id}, Help");
    }
    
    public MenuItem InitMainMenu()
    {
        var mainMenu = new MenuItem("MainMenu", null)
        {
            UploadHandlerCallback = HandlerMenuDefault,
        };

        var languageMenu = new MenuItem("Language", mainMenu)
        {
            UploadHandlerCallback = HandlerMenuLanguage
        };
        
        var helpMenu = new MenuItem("Help", mainMenu)
        {
            UploadHandlerCallback = HandlerMenuHelp
        };
        
        mainMenu.Children.Add(languageMenu);
        mainMenu.Children.Add(helpMenu);
        
        return mainMenu;
    }
}