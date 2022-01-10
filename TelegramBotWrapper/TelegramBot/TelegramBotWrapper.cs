using Microsoft.Extensions.Logging;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot.Wrapper.UserRegistry;

namespace Telegram.Bot.Wrapper.TelegramBot;

public class TelegramBotWrapper : ITelegramBotWrapper
{
    private MenuItem _mainMenu = new("MainMenu", null);
    private readonly ILogger<TelegramBotWrapper> _logger;
    private readonly IUserStateManager _userState;
    private readonly ITelegramBotClient _botClient;
    private readonly ICallbackStorage _callbackStorage;

    public TelegramBotWrapper(ITelegramBotClient botClient,
        IUserStateManager userState,
        ILogger<TelegramBotWrapper> logger, ICallbackStorage callbackStorage)
    {
        _botClient = botClient;
        _userState = userState;
        _logger = logger;
        _callbackStorage = callbackStorage;
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
            currentMenu = subItem.Type == MenuItemType.Back ? subItem.Parent?.Parent : subItem;
            if (currentMenu == null)
            {
                _logger.LogError($"Failed to find menu with name {actualMenuName}");
                return false;
            }
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

    private ReplyKeyboardMarkup RenderMenu(MenuItem menu)
    {
        var showMenuItem = menu.ShowMenuItem();
        if (showMenuItem == null)
        {
            _logger.LogError("Error render menu item");
            throw new ArgumentNullException(nameof(showMenuItem));
        }

        var buttons = new List<List<KeyboardButton>>();
        var row = new List<KeyboardButton>();
        buttons.Add(row);

        foreach (var child in showMenuItem.Children)
        {
            if (child.Type == MenuItemType.IsRequestPhoneButton)
                row.Add(KeyboardButton.WithRequestContact(child.Name));
            else
                row.Add(new KeyboardButton(child.Name));
            if (!child.LastInRow) continue;
            row = new List<KeyboardButton>();
            buttons.Add(row);
        }

        var res = new ReplyKeyboardMarkup(buttons.ToArray())
        {
            ResizeKeyboard = true,
            OneTimeKeyboard = false
        };

        return res;
    }

    private InlineKeyboardMarkup RenderInlineMenu(InlineMenu menu)
    {
        var row = new List<InlineKeyboardButton>();
        foreach (var item in menu.Items)
        {
            var uid = Guid.NewGuid().ToString("N");
            row.Add(new InlineKeyboardButton(item.ItemName)
            {
                CallbackData = uid
            });
            if (item.Callback != null)
                _callbackStorage.AddCallBack(uid, item, item.Callback);
        }

        InlineKeyboardMarkup res = row.ToArray();
        return res;
    }

    public async Task SendText(UserProfile user, string text)
    {
        await _botClient.SendTextMessageAsync(user.Id, text, ParseMode.Html);
    }

    public async Task SendMenu(UserProfile user, string text, MenuItem menu)
    {
        var renderedMenu = RenderMenu(menu);
        await _botClient.SendTextMessageAsync(user.Id, text, replyMarkup: renderedMenu);
    }

    public async Task SendInlineMenu(UserProfile user, string text, InlineMenu inlineMenu)
    {
        await _botClient.SendChatActionAsync(user.Id, ChatAction.Typing);

        InlineKeyboardMarkup inlineKeyboard = RenderInlineMenu(inlineMenu);

        await _botClient.SendTextMessageAsync(chatId: user.Id,
            text: text,
            replyMarkup: inlineKeyboard);
    }
}