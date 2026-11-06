namespace MenuSystem;

/// <summary>
/// Strings that a menu item action returns to tell the menu what to do next.
/// Any other string (for example <see cref="None"/>) means "show this menu again".
/// </summary>
public static class MenuResult
{
    /// <summary>Nothing special happened, show the same menu again.</summary>
    public const string None = "";

    /// <summary>Close only the menu that ran the action.</summary>
    public const string Back = "back";

    /// <summary>Close every menu above the main menu.</summary>
    public const string ReturnToMain = "main";

    /// <summary>Close every menu, including the main menu.</summary>
    public const string Exit = "exit";
}
