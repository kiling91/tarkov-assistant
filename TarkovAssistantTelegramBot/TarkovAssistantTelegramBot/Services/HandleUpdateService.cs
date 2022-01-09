using Tarkov.Assistant.Telegram.Bot.TelegramBotWrapper;
using Tarkov.Assistant.Telegram.Bot.UserRegistry;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Tarkov.Assistant.Telegram.Bot.Services;

public class HandleUpdateService
{
    private readonly ITelegramBotWrapper _telegramBot;
    private readonly IUserRegistry _userRegistry;
    private readonly ITelegramBotController _controller;
    private readonly ILogger<HandleUpdateService> _logger;

    public HandleUpdateService(IUserRegistry userRegistry, 
        ITelegramBotWrapper telegramBot,
        ITelegramBotController controller,
        ILogger<HandleUpdateService> logger )
    {
        _telegramBot = telegramBot;
        _controller = controller;
        _userRegistry = userRegistry;
        _logger = logger;
        _telegramBot.SetupMainMenu(_controller.InitMainMenu());
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
            UpdateType.Message            => BotOnMessageReceived(update.Message!),
            //UpdateType.EditedMessage      => BotOnMessageReceived(update.EditedMessage!),
            //UpdateType.CallbackQuery      => BotOnCallbackQueryReceived(update.CallbackQuery!),
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
        
        if (!await _telegramBot.DrawMenu(message.Text, user))
        {
            // TODO - если не нашли меню, то обрабатываем пользовательский ввод
            await _telegramBot.DrawMainMenu(user);
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
