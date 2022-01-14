using Microsoft.Extensions.Logging;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InputFiles;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot.Wrapper.UserRegistry;

namespace Telegram.Bot.Wrapper.TelegramBot;

public class TelegramBotWrapper : ITelegramBotWrapper
{
    private MenuItem _mainMenu = new("MainMenu", null);
    private readonly ILogger<TelegramBotWrapper> _logger;
    private readonly IUserStateManager _userState;
    private readonly ITelegramBotClient _botClient;

    public TelegramBotWrapper(
        ITelegramBotClient botClient,
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
        var actualMenuName = await _userState.GetActualMenuName(user.Id);
        var currentMenu = actualMenuName == "" ? _mainMenu : _mainMenu.FindMenu(actualMenuName);

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

        await _userState.SetActualMenuName(user.Id, currentMenu.Name);

        if (currentMenu.HandlerCallback != null)
        {
            await currentMenu.HandlerCallback(currentMenu, user);
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
            row.Add(child.Type == MenuItemType.IsRequestPhoneButton
                ? KeyboardButton.WithRequestContact(child.Name)
                : new KeyboardButton(child.Name));
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

    private async Task<InlineKeyboardMarkup> RenderInlineMenu(long userId, InlineMenu menu)
    {
        if (menu.RemovePrevInlineMenuData)
            await _userState.RemoveInlineMenuData(userId, menu.Key);

        var buttons = new List<List<InlineKeyboardButton>>();
        var row = new List<InlineKeyboardButton>();
        buttons.Add(row);

        foreach (var item in menu.Items)
        {
            if (row.Count >= menu.ItemsPerRow)
            {
                row = new List<InlineKeyboardButton>();
                buttons.Add(row);
            }

            var uid = Guid.NewGuid().ToString("N");
            await _userState.SetInlineMenuData(userId, menu.Key, uid, item.Data);
            row.Add(new InlineKeyboardButton(item.ItemName)
            {
                CallbackData = uid
            });
        }

        return buttons.ToArray();
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

        var inlineKeyboard = await RenderInlineMenu(user.Id, inlineMenu);

        await _botClient.SendTextMessageAsync(chatId: user.Id,
            text: text,
            replyMarkup: inlineKeyboard);
    }

    public async Task SendPhoto(UserProfile user, string filePath, string text, InlineMenu? inlineMenu = null)
    {
        var inlineKeyboard = inlineMenu != null ? await RenderInlineMenu(user.Id, inlineMenu) : null;
        
        await _botClient.SendChatActionAsync(user.Id, ChatAction.UploadPhoto);
        await using FileStream fileStream = new(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
        var fileName = filePath.Split(Path.DirectorySeparatorChar).Last();
        await _botClient.SendPhotoAsync(chatId: user.Id,
            photo: new InputOnlineFile(fileStream, fileName),
            caption: text, ParseMode.Html,
            replyMarkup: inlineKeyboard);
    }
}