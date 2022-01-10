using Microsoft.Extensions.Logging;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot.Wrapper.UserRegistry;

namespace Telegram.Bot.Wrapper.TelegramBot;

public class TelegramBotWrapper: ITelegramBotWrapper
{
    private MenuItem _mainMenu = new ("MainMenu", null);
    private readonly ILogger<TelegramBotWrapper> _logger;
    private readonly IUserStateManager _userState;
    private readonly ITelegramBotClient _botClient;
    
    public TelegramBotWrapper(ITelegramBotClient botClient,
        IUserStateManager userState, 
        ILogger<TelegramBotWrapper> logger)
    {
        _botClient = botClient;
        _userState = userState;
        _logger = logger;
    }
    
    public void SetupMainMenu(MenuItem mainMenu)
    {
        _mainMenu = mainMenu;
    }

    public async Task<bool> DrawMenu(string? text, UserProfile user)
    {
        await Task.CompletedTask;
        if (!_mainMenu.Children.Any())
            return false;
        if (text == null)
            // TODO обработать пустой текст
            return false;
        
        // Получаем актуальное меню, в котором сейчас находимся
        var actualMenuName = _userState.GetActualMenuName(user.Id);
        MenuItem? currentMenu = null;
        currentMenu = actualMenuName == "" ? _mainMenu : _mainMenu.FindMenu(actualMenuName);
        
        if (currentMenu == null) 
        {
             _logger.LogError($"Failed to find menu with name {actualMenuName}");
             return false;
        }
        
        var subItem = currentMenu.FindSubMenu(text);
        if (subItem != null)
        {
            // if subItem.Params.IsMenuUpButton {
            //    currentMenu = subItem.Parent.Parent
            currentMenu = subItem;
        }
        else
        {
            _logger.LogError($"Failed to find menu with name {actualMenuName}");
            return false;
        }

        _userState.SetActualMenuName(user.Id, currentMenu.Name);

        if (currentMenu.HandlerCallback != null)
        {
            currentMenu.HandlerCallback(currentMenu, user);
        }
        
        return true;
    }
    
    public Task DrawMainMenu(UserProfile user)
    {
        _userState.SetActualMenuName(user.Id, "");
        if (_mainMenu.HandlerCallback != null)
        {
            _mainMenu.HandlerCallback(_mainMenu, user);
        }
        return Task.CompletedTask;
    }

    private ReplyKeyboardMarkup RenderMenuItem(MenuItem menu)
    {
        var buttons = new List<List<KeyboardButton>>();
        var row = new List<KeyboardButton>();
        buttons.Add(row);
        
        foreach (var child in menu.Children)
        {
            if (child.Type == MenuItemType.IsRequestPhoneButton)
            {
                row.Add(KeyboardButton.WithRequestContact(child.Name));
            }
            else
            {
                row.Add(new KeyboardButton(child.Name));
            }

            if (child.LastInRow)
            {
                row = new List<KeyboardButton>();
                buttons.Add(row);
            }
        }
        var res = new ReplyKeyboardMarkup(buttons.ToArray())
        {
            ResizeKeyboard = true,
            OneTimeKeyboard = false
        };
        
        return res;
    }

    public async Task Send(UserProfile user, string text, MenuItem? menu = null)
    {
        if (menu != null)
        {
            var renderedMenu = RenderMenuItem(menu);
            await _botClient.SendTextMessageAsync(user.Id, text, ParseMode.Html,
                replyMarkup: renderedMenu);
        }
        else
        {
            await _botClient.SendTextMessageAsync(user.Id, text, ParseMode.Html);
        }
    }
}