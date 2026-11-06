namespace MenuSystem;

/// <summary>
/// One line in a menu: a shortcut the user types, a title that is shown and an action that runs.
/// </summary>
public class MenuItem
{
    public MenuItem(string shortcut, string title, Func<string> action)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(shortcut);
        ArgumentNullException.ThrowIfNull(action);

        Shortcut = shortcut.Trim().ToUpperInvariant();
        Title = title;
        Action = action;
    }

    /// <summary>Stored in upper case, so "x" and "X" are the same shortcut.</summary>
    public string Shortcut { get; }

    /// <summary>The label. Settable, so it can be changed after the menu is built.</summary>
    public string Title { get; set; }

    /// <summary>Optional. When set, the label is recalculated every time the menu is drawn.</summary>
    public Func<string>? TitleProvider { get; init; }

    /// <summary>What the item does. The returned string is one of <see cref="MenuResult"/> or anything else.</summary>
    public Func<string> Action { get; }

    public string GetTitle() => TitleProvider?.Invoke() ?? Title;

    public override string ToString() => $"{Shortcut}) {GetTitle()}";
}
