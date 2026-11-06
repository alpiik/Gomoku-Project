namespace MenuSystem;

/// <summary>
/// How deep a menu sits. The level decides which mandatory items the menu adds by itself.
/// </summary>
public enum EMenuLevel
{
    /// <summary>First level. Mandatory items: Exit.</summary>
    Main,

    /// <summary>Second level. Mandatory items: Return to previous, Exit.</summary>
    Second,

    /// <summary>Third level and every level below it. Mandatory items: Return to previous, Return to main, Exit.</summary>
    Deeper
}
