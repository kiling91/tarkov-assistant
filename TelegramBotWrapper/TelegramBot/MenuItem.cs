using Telegram.Bot.Wrapper.UserRegistry;

namespace Telegram.Bot.Wrapper.TelegramBot;

public enum MenuItemType
{
    Text,
    IsRequestPhoneButton,
    Back
}

public class MenuItem
{
    public MenuItem(string name, MenuItem? parent)
    {
        Name = name;
        Parent = parent;
    }
    public string Name { get; private set; }
    public List<MenuItem> Children { get; private set; } = new();
    public MenuItem? Parent { get; private set; } = null;
    public Action<MenuItem, UserProfile>? HandlerCallback { get; init; } = null;

    public bool LastInRow { get; init; } = false;

    public MenuItemType Type { get; init; } = MenuItemType.Text;
    
    public MenuItem AddItem(string menuName, Action<MenuItem, UserProfile>? handler, 
        MenuItemType type = MenuItemType.Text, bool lastInRow = false )
    {
        var child = new MenuItem(menuName, this)
        {
            HandlerCallback = handler,
            LastInRow = lastInRow,
            Type = type,
        };
        Children.Add(child);
        return child;
    }
    
    public MenuItem? FindMenu(string menuName)
    {
        foreach (var child in this.Children)
        {
            if (child.Name == menuName)
                return child;
        }
        
        foreach (var child in this.Children)
        {
            var findItem = child.FindMenu(menuName);
            if (findItem != null)
                return findItem;
        }
        return null;
    }

    public MenuItem? FindSubMenu(string menuName)
    {
        return Children.FirstOrDefault(x => x.Name == menuName);
    }
}