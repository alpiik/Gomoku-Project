namespace GameEngine;

/// <summary>
/// Stores games so they can be continued. The game's <see cref="GameState.Id"/> is the key.
/// Every implementation must behave the same way.
/// </summary>
public interface IGameRepository
{
    /// <summary>A short summary of every saved game, the most recently saved first. Empty list when nothing is stored.</summary>
    List<GameSummary> List();

    /// <summary>The full state, ready to continue. Throws <see cref="KeyNotFoundException"/> when there is no such game.</summary>
    GameState Get(Guid id);

    /// <summary>Inserts a new game or overwrites the one with the same id. Never creates duplicates.</summary>
    void Save(GameState state);

    /// <summary>Does nothing when there is no such game.</summary>
    void Delete(Guid id);
}

/// <summary>What the saved games list shows, without loading every full game.</summary>
public record GameSummary(Guid Id, string Name, string ConfigName, DateTime SavedAtUtc);
