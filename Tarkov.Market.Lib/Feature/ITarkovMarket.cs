namespace Tarkov.Market.Lib.Feature;

public struct SearchByName
{
    public int AllCount { get; set; }
    public int MainTagsCount { get; set; }
    public IEnumerable<TarkovItem> Items { get; set; }
}

public interface ITarkovMarket
{
    void LodItems(string baseFolder);
    SearchByName SearchByName(string query, string lang, int skip, int take, string? tag);
}