namespace Tarkov.Market.Lib.Feature;

public interface ITarkovMarket
{
    void LodItems(string baseFolder);
    (int, IEnumerable<TarkovItem>) SearchByName(string query, string lang, int skip, int take, string? tag);
}