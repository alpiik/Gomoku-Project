namespace MenuSystem;

/// <summary>
/// A console menu. It owns the input loop, adds the mandatory items for its level and
/// rejects duplicate shortcuts. A submenu is a <see cref="MenuItem"/> whose action is the
/// submenu's <see cref="Run"/> method, so menus can be nested to any depth.
/// </summary>
public class Menu
{
    private readonly string _title;
    private readonly EMenuLevel _level;
    private readonly List<MenuItem> _items = [];
    private readonly List<MenuItem> _mandatoryItems;

    public Menu(string title, EMenuLevel level)
    {
        _title = title;
        _level = level;
        _mandatoryItems = CreateMandatoryItems(level);
    }

    /// <summary>
    /// Adds an item. Throws <see cref="ArgumentException"/> when the shortcut is already used
    /// in this menu, including by one of the mandatory items.
    /// </summary>
    public Menu AddItem(MenuItem item)
    {
        ArgumentNullException.ThrowIfNull(item);
        if (FindItem(item.Shortcut) is not null)
        {
            throw new ArgumentException($"Shortcut '{item.Shortcut}' is already used in menu '{_title}'.", nameof(item));
        }

        _items.Add(item);
        return this;
    }

    public Menu AddItem(string shortcut, string title, Func<string> action) =>
        AddItem(new MenuItem(shortcut, title, action));

    /// <summary>
    /// Shows the menu until the user leaves it. Returns <see cref="MenuResult.Exit"/> or
    /// <see cref="MenuResult.ReturnToMain"/> when the menus above must close too, otherwise
    /// <see cref="MenuResult.None"/>.
    /// </summary>
    public string Run()
    {
        var error = "";
        while (true)
        {
            Draw(error);
            var input = Console.ReadLine();
            if (input is null)
            {
                // end of input (Ctrl+Z / Ctrl+D): nothing more will ever be typed
                return MenuResult.Exit;
            }

            var item = FindItem(input.Trim().ToUpperInvariant());
            if (item is null)
            {
                error = $"Unknown choice '{input.Trim()}'. Type one of the shortcuts above.";
                continue;
            }

            error = "";
            switch (item.Action())
            {
                case MenuResult.Exit:
                    return MenuResult.Exit;
                case MenuResult.ReturnToMain when _level != EMenuLevel.Main:
                    return MenuResult.ReturnToMain;
                case MenuResult.Back:
                    // closes only this menu, so the menu above gets None and stays open
                    return MenuResult.None;
            }
        }
    }

    private MenuItem? FindItem(string shortcut) =>
        _items.Find(i => i.Shortcut == shortcut) ?? _mandatoryItems.Find(i => i.Shortcut == shortcut);

    private void Draw(string error)
    {
        if (!Console.IsOutputRedirected)
        {
            Console.Clear();
        }

        Console.WriteLine(_title);
        Console.WriteLine(new string('=', _title.Length));
        foreach (var item in _items)
        {
            Console.WriteLine(item);
        }

        Console.WriteLine("--------------------");
        foreach (var item in _mandatoryItems)
        {
            Console.WriteLine(item);
        }

        if (error != "")
        {
            Console.WriteLine();
            Console.WriteLine(error);
        }

        Console.Write("> ");
    }

    private static List<MenuItem> CreateMandatoryItems(EMenuLevel level)
    {
        List<MenuItem> items = [];
        if (level != EMenuLevel.Main)
        {
            items.Add(new MenuItem(MenuShortcuts.Back, "Return to previous", () => MenuResult.Back));
        }

        if (level == EMenuLevel.Deeper)
        {
            items.Add(new MenuItem(MenuShortcuts.ReturnToMain, "Return to main", () => MenuResult.ReturnToMain));
        }

        items.Add(new MenuItem(MenuShortcuts.Exit, "Exit", () => MenuResult.Exit));
        return items;
    }
}
