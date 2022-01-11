using System.Globalization;
using MediatR;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using Tarkov.Assistant.Telegram.Bot.Command;
using Telegram.Bot.Wrapper;
using Telegram.Bot.Wrapper.TelegramBot;
using Telegram.Bot.Wrapper.UserRegistry;

namespace Tarkov.Assistant.Telegram.Bot.Feature;

public class TelegramBotController : ITelegramBotController
{
    private readonly ITelegramBotWrapper _tg;
    private readonly IStringLocalizer<TelegramBotController> _localizer;
    private readonly ILogger<TelegramBotController> _logger;
    private readonly IMediator _mediator;
    
    public TelegramBotController(ITelegramBotWrapper telegramBot,
        IUserRegistry userRegistry,
        ILogger<TelegramBotController> logger,
        IStringLocalizer<TelegramBotController> localizer, 
        IMediator mediator)
    {
        _tg = telegramBot;
        _logger = logger;
        _localizer = localizer;
        _mediator = mediator;
    }

    private void HandlerMenuDefault(MenuItem menu, UserProfile user)
    {
        _logger.LogInformation($"User: {user.Id}, Menu: {menu.Name}"); 
        _tg.SendMenu(user, $"User: {user.Id}, Menu: {menu.Name}", menu);
    }
    
    private void HandlerMenuLanguage(MenuItem menu, UserProfile user)
    {
        _mediator.Send(new GetOrderProduct.Query(user));
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