using Telegram.Bot.Wrapper.UserRegistry;

namespace Telegram.Bot.Wrapper.TelegramBot;

public class InlineMenuItem
{
	public string ItemName { get; init; } = "";
	public string? Url { get; init; }
	public object? Data { get; set; }
	public string? Key { get; set; }
	public Func<InlineMenuItem, UserProfile, Task>? Callback { get; init; } = null;
}

public class InlineMenu
{
	public int ItemsPerRow { get; init; } = 1;
	public List<InlineMenuItem> Items { get; private set; } = new List<InlineMenuItem>();
}
