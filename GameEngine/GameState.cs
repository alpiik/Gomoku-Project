namespace GameEngine;

/// <summary>
/// Everything needed to continue a game. Plain public properties so it serialises to JSON as is.
/// The status (won / draw) is not stored: it is calculated from the board by <see cref="GameBrain"/>.
/// </summary>
public class GameState
{
    public const int MaxNameLength = 64;

    /// <summary>The key of a saved game: the same in JSON files, the database and (later) web URLs.</summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>Shown in the list of saved games.</summary>
    public string Name { get; set; } = "";

    /// <summary>A copy of the configuration, so a saved game still works if the configuration is edited later.</summary>
    public required GameConfiguration Config { get; init; }

    /// <summary>Rows first, then columns: <c>Board[row][col]</c>. Jagged, because JSON cannot store <c>[,]</c>.</summary>
    public EGamePiece[][] Board { get; set; } = [];

    public EGamePiece NextMoveBy { get; set; } = EGamePiece.X;

    /// <summary>Who plays X. Saves from before A5 don't have it and load as human.</summary>
    public EPlayerType PlayerX { get; set; } = EPlayerType.Human;

    /// <summary>Who plays O. Saves from before A5 don't have it and load as human.</summary>
    public EPlayerType PlayerO { get; set; } = EPlayerType.Human;

    /// <summary>Every move in order. The last one is highlighted and is where the win check starts.</summary>
    public List<Move> Moves { get; set; } = [];

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    /// <summary>Set by the repository every time the game is saved. The saved games list is sorted by it.</summary>
    public DateTime SavedAtUtc { get; set; }

    /// <summary>A new, empty game. Without a name the game is named after its configuration.</summary>
    public static GameState New(GameConfiguration config, string? name = null,
        EPlayerType playerX = EPlayerType.Human, EPlayerType playerO = EPlayerType.Human) => new()
    {
        Name = string.IsNullOrWhiteSpace(name) ? config.Name : name.Trim(),
        Config = config,
        Board = CreateBoard(config.BoardHeight, config.BoardWidth),
        PlayerX = playerX,
        PlayerO = playerO,
    };

    public EPlayerType PlayerOf(EGamePiece piece) => piece == EGamePiece.X ? PlayerX : PlayerO;

    /// <summary>
    /// Problems that would stop the rules from working, e.g. in a hand-edited or damaged save file.
    /// Empty list = the state can be played.
    /// </summary>
    public List<string> Validate()
    {
        var errors = Config.Validate();
        if (Board.Length != Config.BoardHeight || Board.Any(row => row is null || row.Length != Config.BoardWidth))
        {
            errors.Add($"The board must have {Config.BoardHeight} rows of {Config.BoardWidth} cells.");
        }

        if (NextMoveBy is not (EGamePiece.X or EGamePiece.O))
        {
            errors.Add("The next player must be X or O.");
        }

        if (Moves.Any(m => m.Row < 0 || m.Row >= Config.BoardHeight || m.Col < 0 || m.Col >= Config.BoardWidth))
        {
            errors.Add("A move in the history is outside the board.");
        }

        return errors;
    }

    public static EGamePiece[][] CreateBoard(int height, int width)
    {
        var board = new EGamePiece[height][];
        for (var row = 0; row < height; row++)
        {
            board[row] = new EGamePiece[width];
        }

        return board;
    }
}
