using Microsoft.Extensions.Logging;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Wrapper.TelegramBot;
using Telegram.Bot.Wrapper.UserRegistry;

namespace Telegram.Bot.Wrapper.Services;

public class HandleUpdateService : IHandleUpdateService
{
    private readonly ITelegramBotWrapper _botWrapper;
    private readonly IUserRegistry _userRegistry;
    private readonly IUserStateManager _userState;
    private readonly ILogger<HandleUpdateService> _logger;
    private readonly ITelegramBotController _controller;

    public HandleUpdateService(IUserRegistry userRegistry,
        IUserStateManager userState,
        ITelegramBotWrapper botWrapper,
        ITelegramBotController controller,
        ILogger<HandleUpdateService> logger)
    {
        _botWrapper = botWrapper;
        _botWrapper.SetupMainMenu(controller.InitMainMenu());
        _controller = controller;
        _userRegistry = userRegistry;
        _logger = logger;
        _userState = userState;
    }

    public async Task HandleAsync(Update update, CancellationToken ct)
    {
        var handler = update.Type switch
        {
            // UpdateType.Unknown:
            // UpdateType.ChannelPost:
            // UpdateType.EditedChannelPost:
            // UpdateType.ShippingQuery:
            // UpdateType.PreCheckoutQuery:
            // UpdateType.Poll:
            UpdateType.Message => BotOnMessageReceived(update.Message!, ct),
            //UpdateType.EditedMessage      => BotOnMessageReceived(update.EditedMessage!),
            UpdateType.CallbackQuery => BotOnCallbackQueryReceived(update.CallbackQuery!, ct),
            //UpdateType.InlineQuery        => BotOnInlineQueryReceived(update.InlineQuery!),
            //UpdateType.ChosenInlineResult => BotOnChosenInlineResultReceived(update.ChosenInlineResult!),
            _ => OnDefaultCallBack(update.Type)
        };

        try
        {
            await handler;
        }
        catch (Exception exception)
        {
            await HandleErrorAsync(exception);
        }
    }

    private async Task OnDefaultCallBack(UpdateType type)
    {
        _logger.LogWarning($"Not found UpdateType: {type}");
        await Task.CompletedTask;
    }

    private async Task BotOnCallbackQueryReceived(CallbackQuery callbackQuery, CancellationToken ct)
    {
        await Task.CompletedTask;
        if (callbackQuery.Data != null)
        {
            var userId = callbackQuery.From?.Id ?? -1;
            var user = await _userRegistry.FindUser(userId);
            if (user == null)
            {
                _logger.LogError("User is not found");
                return;
            }

            if (callbackQuery.Data == null)
            {
                _logger.LogError("Call back data not found");
                return;
            }
            await _controller.OnBeforeCallBack(user);
            var data = await _userState.GetInlineMenuData(userId, callbackQuery.Data);
            if (!data.Exist)
                return;

            await _controller.OnInlineMenuCallBack(user, data.Key!, data.Data);
        }
    }

    private async Task BotOnMessageReceived(Message message, CancellationToken ct)
    {
        if (message.Type != MessageType.Text)
            return;

        var from = message.From;
        if (from == null)
        {
            _logger.LogError("User is not found");
            return;
        }

        var user = await _userRegistry.FindOrCreateUser(from.Id,
            from.FirstName, from.LastName,
            from.LanguageCode);
        
        if (message.Text == "/start")
        {
            await _botWrapper.DrawMainMenu(user);
            return;
        }
        
        if (!await _botWrapper.DrawMenu(message.Text, user))
        {
            if (message.Text != null)
                if (await _controller.OnUserInputCallBack(user, message.Text))
                    return;
            await _botWrapper.DrawMainMenu(user);
        }
    }

    private Task HandleErrorAsync(Exception exception)
    {
        if (exception is NeedReloadLanguageException dfd)
            throw new NeedReloadLanguageException(dfd.Lang);

        var errorMessage = exception switch
        {
            ApiRequestException apiRequestException =>
                $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
            _ => exception.ToString()
        };

        _logger.LogError("HandleError: {ErrorMessage}", errorMessage);
        return Task.CompletedTask;
    }
}