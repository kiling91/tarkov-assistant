using Microsoft.Extensions.Logging;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Wrapper.TelegramBot;
using Telegram.Bot.Wrapper.UserRegistry;

namespace Telegram.Bot.Wrapper.Services;

public class HandleUpdateService
{
    private readonly ITelegramBotWrapper _botWrapper;
    private readonly IUserRegistry _userRegistry;
    private readonly ILogger<HandleUpdateService> _logger;
    private readonly ICallbackStorage _callbackStorage;
    
    public HandleUpdateService(IUserRegistry userRegistry,
        ITelegramBotWrapper botWrapper,
        ITelegramBotController controller,
        ICallbackStorage callbackStorage,
        ILogger<HandleUpdateService> logger )
    {
        _botWrapper = botWrapper;
        _botWrapper.SetupMainMenu(controller.InitMainMenu());
        
        _userRegistry = userRegistry;
        _callbackStorage = callbackStorage;

        _logger = logger;
    }

    public async Task HandleAsync(Update update)
    {
        var handler = update.Type switch
        {
            // UpdateType.Unknown:
            // UpdateType.ChannelPost:
            // UpdateType.EditedChannelPost:
            // UpdateType.ShippingQuery:
            // UpdateType.PreCheckoutQuery:
            // UpdateType.Poll:
            UpdateType.Message => BotOnMessageReceived(update.Message!),
            //UpdateType.EditedMessage      => BotOnMessageReceived(update.EditedMessage!),
            UpdateType.CallbackQuery => BotOnCallbackQueryReceived(update.CallbackQuery!),
            //UpdateType.InlineQuery        => BotOnInlineQueryReceived(update.InlineQuery!),
            //UpdateType.ChosenInlineResult => BotOnChosenInlineResultReceived(update.ChosenInlineResult!),
            _ => throw new ArgumentOutOfRangeException()
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

    private async Task BotOnCallbackQueryReceived(CallbackQuery callbackQuery)
    {
        await Task.CompletedTask;
        if (callbackQuery.Data != null)
        {
            var userId = callbackQuery.From?.Id ?? -1;
            if (await _callbackStorage.Invoke(callbackQuery.Data, userId) == false)
                _logger.LogWarning($"Not found callback for user {userId}");
        }
    }

    private async Task BotOnMessageReceived(Message message)
    {
        _logger.LogInformation("Receive message type: {messageType}", message.Type);
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

        if (!await _botWrapper.DrawMenu(message.Text, user))
        {
            // TODO - если не нашли меню, то обрабатываем пользовательский ввод
            await _botWrapper.DrawMainMenu(user);
        }
    }

    private Task HandleErrorAsync(Exception exception)
    {
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