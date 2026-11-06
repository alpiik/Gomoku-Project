using GameEngine;

namespace DAL.Json;

/// <summary>
/// What one .game.json file contains: the game state as the engine uses it, plus information that
/// only matters for the file. The engine never sees this class.
/// </summary>
public class SavedGameFile
{
    /// <summary>Which layout of the file this is, see <see cref="SaveFormat"/>.</summary>
    public int Version { get; set; }

    /// <summary>When the file was written. The saved games list is sorted by it.</summary>
    public DateTime SavedAtUtc { get; set; }

    public required GameState State { get; set; }
}
