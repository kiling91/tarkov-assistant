using Telegram.Bot.Wrapper.UserRegistry;

namespace Telegram.Bot.Wrapper.TelegramBot;

public class InlineMenuItem
{
	public InlineMenuItem(string itemName)
	{
		ItemName = itemName;
	}

	public string ItemName { get; init; }
	
	public string? Url { get; init; }
	public string? Data { get; init; }
}

public class InlineMenu
{
	public InlineMenu(string key)
	{
		Key = key;
	}

	public int ItemsPerRow { get; init; } = 1;
	
	public bool RemovePrevInlineMenuData { get; init; } = true;
	public string Key { get; init; }
	public List<InlineMenuItem> Items { get; private set; } = new List<InlineMenuItem>();
}
