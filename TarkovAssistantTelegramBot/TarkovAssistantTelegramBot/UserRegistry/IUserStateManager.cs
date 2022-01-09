namespace Tarkov.Assistant.Telegram.Bot.UserRegistry;

public interface IUserStateManager
{
    void SetActualMenuName(long userId, string menuName);
    string GetActualMenuName(long userId);
}