using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Tarkov.Assistant.Telegram.Bot.Command;
using Telegram.Bot.Wrapper;
using File = System.IO.File;

namespace Tarkov.Assistant.Telegram.Bot.TarkovMarket;


public class TarkovMarket: ITarkovMarket
{
    private readonly List<TarkovItem> _items = new();
    private readonly Dictionary<string, Dictionary<string, string>> _translations = new();
    private readonly IOptions<AvailableLanguagesConfiguration> _languages;

    public TarkovMarket(IOptions<AvailableLanguagesConfiguration> languages)
    {
        _languages = languages;
    }

    public void LodItems()
    {
        var baseFolder = @"C:\Users\kiling\projects\tarkov-assistant\tarkov-market-parser";
        
        var data = File.ReadAllText(Path.Join(baseFolder, @"tarkov_items.json"));
        
        var items = JsonConvert.DeserializeObject<List<TarkovItem>>(data);
        if (items != null)
            _items.AddRange(items);

        if (_languages.Value.Languages != null)
            foreach (var ln in _languages.Value.Languages)
            {
                var translationData = File.ReadAllText(Path.Join(baseFolder, $"nametouid\\{ln.LanguageCode}.json"));
                var translation = JsonConvert.DeserializeObject<Dictionary<string, string>>(translationData);
                if (translation != null)
                    _translations.Add(ln.LanguageCode, translation);
            }
    }

    public IEnumerable<TarkovItem> SearchByName(string query, string lang)
    {
        if (!_translations.ContainsKey(lang))
        {
           //TODO: совй exeption
           throw new ArgumentNullException();
        }

        var list = new List<string>();
        
        var translation = _translations[lang];
        foreach (var tr in translation)
        {
            var v1 = tr.Key.ToLower().Trim();
            var v2 = query.ToLower().Trim();
            if (!v1.Contains(v2))
                continue;
            list.Add(tr.Value);
        }

        var result = new List<TarkovItem>();
        foreach (var id in list)
        {
            var item = _items.FirstOrDefault(x => x.Uid == id);
            if (item == null)
            {
                //TODO: совй exeption
                throw new ArgumentNullException();
            }
            result.Add(item);
        }

        return result;
    }
}