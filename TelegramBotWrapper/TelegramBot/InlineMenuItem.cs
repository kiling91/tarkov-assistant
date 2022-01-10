using Telegram.Bot.Wrapper.UserRegistry;

namespace Telegram.Bot.Wrapper.TelegramBot;

public class InlineMenuItem
{
	public string? Unique { get; init; }
	public string? ItemId { get; init; }
	public string ItemName { get; init; } = "";
	public string? ItemImage { get; init; }
	public string? Url { get; init; }
	public Action<InlineMenuItem, UserProfile>? Callback { get; init; } = null;
}

public class InlineMenu
{
	public int ItemsPerRow { get; init; } = 1;
	public List<InlineMenuItem> Items { get; private set; } = new List<InlineMenuItem>();
}
