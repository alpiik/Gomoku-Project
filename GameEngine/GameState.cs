namespace GameEngine;

/// <summary>
/// Everything needed to continue a game. Plain public properties so it serialises to JSON as is.
/// The status (won / draw) is not stored: it is calculated from the board by <see cref="GameBrain"/>.
/// </summary>
public class GameState
{
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>A copy of the configuration, so a saved game still works if the configuration is edited later.</summary>
    public required GameConfiguration Config { get; init; }

    /// <summary>Rows first, then columns: <c>Board[row][col]</c>. Jagged, because JSON cannot store <c>[,]</c>.</summary>
    public EGamePiece[][] Board { get; set; } = [];

    public EGamePiece NextMoveBy { get; set; } = EGamePiece.X;

    /// <summary>Every move in order. The last one is highlighted and is where the win check starts.</summary>
    public List<Move> Moves { get; set; } = [];

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public static GameState New(GameConfiguration config) => new()
    {
        Config = config,
        Board = CreateBoard(config.BoardHeight, config.BoardWidth),
    };

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
