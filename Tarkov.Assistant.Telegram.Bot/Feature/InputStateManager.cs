using Telegram.Bot.Wrapper.UserRegistry;

namespace Tarkov.Assistant.Telegram.Bot.Feature;

public class InputStateManager: IInputStateManager
{
    private string _state = "";
    
    public async Task SetInputState(long userId, string state)
    {
        await Task.CompletedTask;
        _state = state;
    }

    public async Task<string> GetInputState(long userId)
    {
        await Task.CompletedTask;
        return _state;
    }
}