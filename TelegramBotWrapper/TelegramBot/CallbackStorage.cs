using Telegram.Bot.Wrapper.UserRegistry;

namespace Telegram.Bot.Wrapper.TelegramBot;

struct CallBackData
{
    public InlineMenuItem Menu { get; private set; }
    public Action<InlineMenuItem, UserProfile> Callback { get; private set; }

    public CallBackData(InlineMenuItem menu, Action<InlineMenuItem, UserProfile> callback)
    {
        this.Menu = menu;
        this.Callback = callback;
    }
}

public class CallbackStorage : ICallbackStorage
{
    private readonly IUserRegistry _userRegistry;
    private readonly Dictionary<string, CallBackData> _storage = new();
    
    public CallbackStorage(IUserRegistry userRegistry)
    {
        _userRegistry = userRegistry;
    }

    public void AddCallBack(string uid, InlineMenuItem menuItem,
        Action<InlineMenuItem, UserProfile> callback)
    {
        _storage.Add(uid, new CallBackData(menuItem, callback));
    }

    public async Task<bool> Invoke(string uid, long userId)
    {
        if (!_storage.ContainsKey(uid))
            return false;

        var item = _storage[uid];
        
        var user = await _userRegistry.FindUser(userId);
        if (user == null)
            return false;
        
        item.Callback(item.Menu, user);
        return true;
    }
}