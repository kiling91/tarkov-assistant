using Microsoft.Extensions.Localization;
using Telegram.Bot.Wrapper;
using Telegram.Bot.Wrapper.TelegramBot;
using Telegram.Bot.Wrapper.UserRegistry;

namespace Tarkov.Assistant.Telegram.Bot.Feature;

public class TelegramBotController : ITelegramBotController
{
    private readonly ITelegramBotWrapper _tg;
    private readonly IStringLocalizer<TelegramBotController> _localizer;
    private readonly ILogger<TelegramBotController> _logger;
    private readonly IUserRegistry _userRegistry;
    private readonly IUserStateManager _userStateManager;
    public TelegramBotController(ITelegramBotWrapper telegramBot,
        IUserRegistry userRegistry,
        IUserStateManager userStateManager,
        ILogger<TelegramBotController> logger,
        IStringLocalizer<TelegramBotController> localizer)
    {
        _tg = telegramBot;
        _userRegistry = userRegistry;
        _userStateManager = userStateManager;
        _logger = logger;
        _localizer = localizer;
    }

    private void HandlerMenuDefault(MenuItem menu, UserProfile user)
    {
        _logger.LogInformation($"User: {user.Id}, Menu: {menu.Name}"); 
        _tg.SendMenu(user, $"User: {user.Id}, Menu: {menu.Name}", menu);
    }

    async Task HandleChangeLanguage(InlineMenuItem menuItem, UserProfile user)
    {
        var newLang = menuItem.Key;
        if (user.Lang != menuItem.Key && newLang != null)
        {
            await _userRegistry.ChangeLang(user.Id, newLang);
            throw new NeedReloadLanguageException(newLang);
        }
        else
        {
            _tg.SetupMainMenu(InitMainMenu());
            await _tg.DrawMainMenu(user);
        }
    }

    private void HandlerMenuLanguage(MenuItem menu, UserProfile user)
    {
        _logger.LogInformation($"User: {user.Id}, Language");

        var inlineMenu = new InlineMenu();
        inlineMenu.Items.Add(new InlineMenuItem()
        {
            ItemName = "Русский",
            Key = "ru",
            Callback = HandleChangeLanguage
        });
        
        inlineMenu.Items.Add(new InlineMenuItem()
        {
            ItemName = "English",
            Key = "en",
            Callback = HandleChangeLanguage
        });
        
        _tg.SendInlineMenu(user, $"Select language", inlineMenu);
    }

    private void HandlerMenuHelp(MenuItem menu, UserProfile user)
    {
        _logger.LogInformation($"User: {user.Id}, Help");
        _tg.SendText(user, $"User: {user.Id}, Help");
    }
    
    private void HandlerMenuPersonalAccount(MenuItem menu, UserProfile user)
    {
        _logger.LogInformation($"User: {user.Id}, Personal account");
        _tg.SendMenu(user, $"User: {user.Id}, Personal account", menu);
    }

    public MenuItem InitMainMenu()
    {
        var mainMenu = new MenuItem("MainMenu", null)
        {
            HandlerCallback = HandlerMenuDefault,
        };
        
        mainMenu.AddItem(_localizer["Share contact"], HandlerMenuHelp, MenuItemType.IsRequestPhoneButton);
        var personalAccount = mainMenu.AddItem(_localizer["Personal account"], 
            HandlerMenuPersonalAccount, MenuItemType.Text, true);

        personalAccount.AddItem(_localizer["Balance"], HandlerMenuHelp);
        personalAccount.AddItem(_localizer["Documents"], HandlerMenuHelp);
        personalAccount.AddItem(_localizer["Back"], null, MenuItemType.Back);

        var lng = _localizer["Language"];
        mainMenu.AddItem(_localizer["Language"], HandlerMenuLanguage);
        mainMenu.AddItem(_localizer["Help"], HandlerMenuHelp, MenuItemType.Text, true);
        
        return mainMenu;
    }
}