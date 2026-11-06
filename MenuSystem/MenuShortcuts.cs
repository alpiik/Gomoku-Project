namespace MenuSystem;

/// <summary>
/// Shortcuts used by the mandatory items. A menu rejects custom items that use the shortcut
/// of one of its own mandatory items.
/// </summary>
public static class MenuShortcuts
{
    /// <summary>Exit - on every level.</summary>
    public const string Exit = "X";

    /// <summary>Return to previous - on the second level and deeper.</summary>
    public const string Back = "B";

    /// <summary>Return to main - on the third level and deeper.</summary>
    public const string ReturnToMain = "M";
}
