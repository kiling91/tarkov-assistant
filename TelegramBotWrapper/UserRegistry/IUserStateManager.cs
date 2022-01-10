namespace Telegram.Bot.Wrapper.UserRegistry;

public interface IUserStateManager
{
    void SetActualMenuName(long userId, string menuName);
    string GetActualMenuName(long userId);
}