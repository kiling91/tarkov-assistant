namespace Tarkov.Market.Lib.Feature;

public interface ITarkovMarket
{
    void LodItems(string baseFolder);
    (int, IEnumerable<TarkovItem>) SearchByName(string query, int skip, int take, string? tag);
}