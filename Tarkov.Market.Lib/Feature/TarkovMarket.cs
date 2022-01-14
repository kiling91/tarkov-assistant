using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Telegram.Bot.Wrapper;
using File = System.IO.File;

namespace Tarkov.Market.Lib.Feature;

public class TarkovMarket : ITarkovMarket
{
    private const string TarkovItems = "tarkov_items.json";
    private const string NameToUidDir = "name_to_uid";
    private readonly List<TarkovItem> _items = new();
    private readonly Dictionary<string, Dictionary<string, string>> _translations = new();
    private readonly IOptions<AvailableLanguagesConfiguration> _languages;

    public TarkovMarket(IOptions<AvailableLanguagesConfiguration> languages)
    {
        _languages = languages;
    }

    public void LodItems(string baseFolder)
    {
        var data = File.ReadAllText(Path.Join(baseFolder, TarkovItems));

        var items = JsonConvert.DeserializeObject<List<TarkovItem>>(data);
        if (items != null)
            _items.AddRange(items);

        if (_languages.Value.Languages != null)
            foreach (var ln in _languages.Value.Languages)
            {
                var translationData =
                    File.ReadAllText(Path.Join(baseFolder, $"{NameToUidDir}\\{ln.LanguageCode}.json"));
                var translation = JsonConvert.DeserializeObject<Dictionary<string, string>>(translationData);
                if (translation != null)
                    _translations.Add(ln.LanguageCode, translation);
            }
    }

    public (int, IEnumerable<TarkovItem>) SearchByName(string query, string lang, int skip, int take, string? tag)
    {
        if (!_translations.ContainsKey(lang))
        {
            //TODO: совй exeption
            throw new ArgumentNullException();
        }

        var uidList = new HashSet<string>();

        var translation = _translations[lang];
        foreach (var tr in translation)
        {
            var v1 = tr.Key.ToLower().Trim();
            var v2 = query.ToLower().Trim();
            if (!v1.Contains(v2))
                continue;
            uidList.Add(tr.Value);
        }

        var result = new List<TarkovItem>();
        foreach (var id in uidList)
        {
            var item = _items.FirstOrDefault(x => x.Uid == id);
            if (item == null)
            {
                //TODO: совй exeption
                throw new ArgumentNullException();
            }

            result.Add(item);
        }

        result.Sort((x, y) =>
            String.Compare(x.Tags!.First(), y.Tags!.First(), StringComparison.Ordinal
            ));
        
        if (tag != null)
            result = result.Where(x => x.Tags != null && x.Tags.Contains(tag)).ToList();
        
        return (result.Count, result.Skip(skip).Take(take));
    }
}