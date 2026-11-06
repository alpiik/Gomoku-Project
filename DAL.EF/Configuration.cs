namespace DAL.EF;

/// <summary>
/// A row of the Configurations table. Not the engine's <c>GameEngine.GameConfiguration</c> record:
/// EF needs settable properties and an int key, the engine needs an immutable value. The repository maps between them.
/// </summary>
public class Configuration
{
    /// <summary>Database key only. Outside DAL.EF a configuration is found by its name.</summary>
    public int Id { get; set; }

    public string Name { get; set; } = "";
    public int BoardWidth { get; set; }
    public int BoardHeight { get; set; }
    public int WinLength { get; set; }
    public bool ExactWinLength { get; set; }
    public bool ProOpening { get; set; }

    /// <summary>The games started from this configuration (the "many" side of the relationship).</summary>
    public ICollection<SavedGame> SavedGames { get; set; } = new List<SavedGame>();
}
