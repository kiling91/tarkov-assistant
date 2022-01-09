using Tarkov.Assistant.Telegram.Bot.UserRegistry;

namespace Tarkov.Assistant.Telegram.Bot.TelegramBotWrapper;

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
    public Action<MenuItem, UserProfile>? UploadHandlerCallback { get; init; } = null;
    
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