namespace Tarkov.Assistant.Telegram.Bot;

public class TelegramBotController: ITelegramBotController
{
    private readonly ITelegramBotWrapper _tg;
    private readonly ILogger<TelegramBotController> _logger;

    public TelegramBotController(ITelegramBotWrapper telegramBot, 
        ILogger<TelegramBotController> logger)
    {
        _tg = telegramBot;
        _logger = logger;
    }

    private void HandlerMenuDefault(MenuItem menu, UserProfile user)
    {
        _logger.LogInformation($"User: {user.Id}, Menu: {menu.Name}");
        _tg.Send(user, $"User: {user.Id}, Menu: {menu.Name}");
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