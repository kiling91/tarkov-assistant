using Telegram.Bot.Types;

namespace Telegram.Bot.Wrapper.Services;

public interface IHandleUpdateService
{
    Task HandleAsync(Update update);
}