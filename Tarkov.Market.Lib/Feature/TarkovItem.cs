using Newtonsoft.Json;

namespace Tarkov.Market.Lib.Feature;

public class TarkovItemTranslation
{
    [JsonProperty("name")] public string? Name { get; set; }
    [JsonProperty("shortName")] public string? ShortName { get; set; }
}

public class TarkovItem
{
    [JsonProperty("uid")] public string? Uid { get; set; }
    [JsonProperty("tags")] public IEnumerable<string>? Tags { get; set; }
    [JsonProperty("basePrice")] public int? BasePrice { get; set; }
    [JsonProperty("avg24hPrice")] public int? Avg24hPrice { get; set; }
    [JsonProperty("avg7daysPrice")] public int? Avg7daysPrice { get; set; }
    [JsonProperty("traderName")] public string? TraderName { get; set; }
    [JsonProperty("traderPrice")] public int? TraderPrice { get; set; }
    [JsonProperty("traderPriceCur")] public string? TraderPriceCur { get; set; }
    [JsonProperty("updated")] public DateTime? Updated { get; set; }
    [JsonProperty("slots")] public int? Slots { get; set; }
    [JsonProperty("diff24h")] public float? Diff24h { get; set; }
    [JsonProperty("diff7days")] public float? Diff7days { get; set; }
    [JsonProperty("iconSm")] public string? IconSm { get; set; }
    [JsonProperty("iconLg")] public string? IconLg { get; set; }
    [JsonProperty("translation")] public Dictionary<string, TarkovItemTranslation>? Translation { get; set; }
}