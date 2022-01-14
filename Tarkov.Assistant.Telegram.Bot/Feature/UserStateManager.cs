using Telegram.Bot.Wrapper.UserRegistry;

namespace Tarkov.Assistant.Telegram.Bot.Feature;

public class UserStateManager : IUserStateManager
{
    private string _menuName = "";
    
    private readonly Dictionary<string, string> _dataStorage = new();
    private readonly Dictionary<string, bool> _uidExists = new();
    private readonly Dictionary<string, string> _keyByUid = new();

    private readonly Dictionary<string, List<string>> _uidsByKey = new();

    public async Task SetActualMenuName(long userId, string menuName)
    {
        await Task.CompletedTask;
        _menuName = menuName;
    }

    public async Task<string> GetActualMenuName(long userId)
    {
        await Task.CompletedTask;
        return _menuName;
    }

    public async Task SetInlineMenuData(long userId, string key, string uid, string? data)
    {
        await Task.CompletedTask;
        if (data != null)
            _dataStorage.Add(uid, data);
        _uidExists.Add(uid, true);
        _keyByUid.Add(uid, key);

        List<string>? list = null;
        if (!_uidsByKey.ContainsKey(key))
            _uidsByKey[key] = new List<string>();
        list = _uidsByKey[key];
        list.Add(uid);
    }

    public async Task<InlineMenuData> GetInlineMenuData(long userId, string uid)
    {
        await Task.CompletedTask;
        return new InlineMenuData
        {
            Data = _dataStorage.ContainsKey(uid) ? _dataStorage[uid] : null,
            Exist = _uidExists.ContainsKey(uid) && _uidExists[uid],
            Key = _keyByUid.ContainsKey(uid) ? _keyByUid[uid] : null
        };
    }

    public async Task RemoveInlineMenuData(long userId, string key)
    {
        await Task.CompletedTask;
        if (!_uidsByKey.TryGetValue(key, out var uids))
            return;

        foreach (var uid in uids)
        {
            _dataStorage.Remove(uid);
            _uidExists.Remove(uid);
            _keyByUid.Remove(uid);
        }
    }
}