namespace GameEngine;

/// <summary>
/// Stores configurations. The name is the key and is compared ignoring case ("small" = "Small").
/// Every implementation must behave the same way.
/// </summary>
public interface IConfigRepository
{
    /// <summary>All configuration names in alphabetical order. Empty list when nothing is stored.</summary>
    List<string> List();

    /// <summary>Throws <see cref="KeyNotFoundException"/> when no configuration has this name.</summary>
    GameConfiguration Get(string name);

    /// <summary>
    /// Inserts a new configuration or overwrites the one with the same name. Never creates duplicates.
    /// Throws <see cref="ArgumentException"/> when <see cref="GameConfiguration.Validate"/> finds problems.
    /// </summary>
    void Save(GameConfiguration config);

    /// <summary>Does nothing when no configuration has this name.</summary>
    void Delete(string name);
}
