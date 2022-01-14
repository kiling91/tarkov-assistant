using Telegram.Bot.Wrapper.UserRegistry;

namespace Tarkov.Assistant.Telegram.Bot.Feature;

public interface IInputStateManager
{
    Task SetInputState(long userId, string state);
    Task<string> GetInputState(long userId);
}