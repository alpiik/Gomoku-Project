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

    /// <summary>
    /// Saves every preset the repository does not have yet. Runs at every start, so on the first run all
    /// four are created and a preset removed from storage by hand comes back. Never overwrites a stored one.
    /// Works with any repository, so JSON and the database are seeded by the same code.
    /// </summary>
    public static void SeedMissing(IConfigRepository repository)
    {
        var existing = repository.List();
        foreach (var preset in All.Where(p => !existing.Contains(p.Name, StringComparer.OrdinalIgnoreCase)))
        {
            repository.Save(preset);
        }
    }
}
