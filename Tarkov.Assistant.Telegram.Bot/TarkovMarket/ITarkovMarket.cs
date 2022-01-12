using Tarkov.Assistant.Telegram.Bot.Command;

namespace Tarkov.Assistant.Telegram.Bot.TarkovMarket;

public interface ITarkovMarket
{
    void LodItems();
    IEnumerable<TarkovItem> SearchByName(string query, string lang);
}