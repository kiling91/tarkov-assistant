namespace TarkovMarket.Feature.TarkovMarket;

public interface ITarkovMarket
{
    void LodItems(string baseFolder);
    (int, IEnumerable<TarkovItem>) SearchByName(string query, string lang, int skip, int take, string? tag);
}