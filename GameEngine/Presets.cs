namespace GameEngine;

/// <summary>The preset configurations required for Gomoku. They cannot be edited or deleted.</summary>
public static class Presets
{
    public static readonly GameConfiguration FreeStyle = new("Free-style", 15, 15, 5);
    public static readonly GameConfiguration Standard = new("Standard", 15, 15, 5) { ExactWinLength = true };
    public static readonly GameConfiguration Small = new("Small", 9, 9, 4);
    public static readonly GameConfiguration Pro = new("Pro", 19, 19, 5) { ProOpening = true };

    public static IReadOnlyList<GameConfiguration> All { get; } = [FreeStyle, Standard, Small, Pro];

    public static bool IsPreset(string name) =>
        All.Any(preset => preset.Name.Equals(name.Trim(), StringComparison.OrdinalIgnoreCase));
}
