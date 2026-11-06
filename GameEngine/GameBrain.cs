using System.Diagnostics.CodeAnalysis;

namespace GameEngine;

/// <summary>
/// The rules of Gomoku. Works on the state it is given; it never creates, loads, saves or draws a state.
/// All coordinates are 0-based.
/// </summary>
public class GameBrain
{
    // the four "forward" directions; CheckWin also walks each of them backwards
    private static readonly (int DRow, int DCol)[] Directions =
    [
        (0, 1),  // horizontal
        (1, 0),  // vertical
        (1, 1),  // diagonal down-right
        (1, -1), // diagonal down-left
    ];

    public GameBrain(GameState state)
    {
        ArgumentNullException.ThrowIfNull(state);
        var errors = state.Validate();
        if (errors.Count > 0)
        {
            throw new ArgumentException(string.Join(" ", errors), nameof(state));
        }

        State = state;
    }

    public GameState State { get; }

    public GameConfiguration Config => State.Config;

    /// <summary>The centre cell. On an even side it is the lower/right one of the two middle cells.</summary>
    public Move Centre => new(Config.BoardHeight / 2, Config.BoardWidth / 2);

    public EGamePiece GetPiece(int row, int col) => State.Board[row][col];

    public bool IsInside(int row, int col) =>
        row >= 0 && row < Config.BoardHeight && col >= 0 && col < Config.BoardWidth;

    /// <summary>Null when the move is legal, otherwise the reason it is not.</summary>
    public string? GetMoveError(int row, int col)
    {
        if (Status != EGameStatus.InProgress)
        {
            return "The game is over.";
        }

        if (!IsInside(row, col))
        {
            return "That cell is outside the board.";
        }

        if (State.Board[row][col] != EGamePiece.Empty)
        {
            return "That cell is taken.";
        }

        return GetProOpeningError(row, col);
    }

    public bool IsLegalMove(int row, int col) => GetMoveError(row, col) is null;

    public List<Move> GetLegalMoves()
    {
        List<Move> moves = [];
        if (Status != EGameStatus.InProgress)
        {
            return moves;
        }

        for (var row = 0; row < Config.BoardHeight; row++)
        {
            for (var col = 0; col < Config.BoardWidth; col++)
            {
                if (State.Board[row][col] == EGamePiece.Empty && GetProOpeningError(row, col) is null)
                {
                    moves.Add(new Move(row, col));
                }
            }
        }

        return moves;
    }

    /// <summary>
    /// The only way the board changes. Throws <see cref="InvalidMoveException"/> for an illegal move,
    /// because callers are expected to check first (or use <see cref="TryMakeMove"/>).
    /// </summary>
    public EGameStatus MakeMove(int row, int col)
    {
        var error = GetMoveError(row, col);
        if (error is not null)
        {
            throw new InvalidMoveException(row, col, error);
        }

        State.Board[row][col] = State.NextMoveBy;
        State.Moves.Add(new Move(row, col));
        State.NextMoveBy = Opponent(State.NextMoveBy);
        return Status;
    }

    /// <summary>For user input: returns false and a message instead of throwing. The turn does not change on false.</summary>
    public bool TryMakeMove(int row, int col, [NotNullWhen(false)] out string? error)
    {
        error = GetMoveError(row, col);
        if (error is not null)
        {
            return false;
        }

        MakeMove(row, col);
        return true;
    }

    public EGameStatus Status
    {
        get
        {
            if (HasWinner())
            {
                var last = State.Moves[^1];
                return State.Board[last.Row][last.Col] == EGamePiece.X ? EGameStatus.XWon : EGameStatus.OWon;
            }

            return IsBoardFull() ? EGameStatus.Draw : EGameStatus.InProgress;
        }
    }

    /// <summary>Only the last move can have completed a line, so only that cell is checked.</summary>
    public bool HasWinner() =>
        State.Moves.Count > 0 && CheckWin(State.Moves[^1].Row, State.Moves[^1].Col);

    public bool IsDraw() => !HasWinner() && IsBoardFull();

    /// <summary>Does the piece at (row, col) form a winning line? Counts outward in all four directions.</summary>
    public bool CheckWin(int row, int col)
    {
        var player = State.Board[row][col];
        if (player == EGamePiece.Empty)
        {
            return false;
        }

        foreach (var (dRow, dCol) in Directions)
        {
            var length = 1
                         + CountDirection(row, col, dRow, dCol, player)
                         + CountDirection(row, col, -dRow, -dCol, player);
            if (IsWinningLength(length))
            {
                return true;
            }
        }

        return false;
    }

    public static EGamePiece Opponent(EGamePiece piece) => piece == EGamePiece.X ? EGamePiece.O : EGamePiece.X;

    // the overline rule: exactly N wins (standard) or N or more wins (free-style)
    private bool IsWinningLength(int length) =>
        Config.ExactWinLength ? length == Config.WinLength : length >= Config.WinLength;

    // how many pieces of the same player follow (row, col) in one direction, not counting (row, col) itself
    private int CountDirection(int row, int col, int dRow, int dCol, EGamePiece player)
    {
        var count = 0;
        var r = row + dRow;
        var c = col + dCol;
        while (IsInside(r, c) && State.Board[r][c] == player)
        {
            count++;
            r += dRow;
            c += dCol;
        }

        return count;
    }

    private bool IsBoardFull()
    {
        foreach (var row in State.Board)
        {
            foreach (var cell in row)
            {
                if (cell == EGamePiece.Empty)
                {
                    return false;
                }
            }
        }

        return true;
    }

    // move 0 is X's first move, move 2 is X's second move
    private string? GetProOpeningError(int row, int col)
    {
        if (!Config.ProOpening)
        {
            return null;
        }

        var centre = Centre;
        var distanceFromCentre = Math.Max(Math.Abs(row - centre.Row), Math.Abs(col - centre.Col));
        return State.Moves.Count switch
        {
            0 when distanceFromCentre != 0 => "Pro opening: the first move must be in the centre.",
            2 when distanceFromCentre < GameConfiguration.ProOpeningDistance =>
                $"Pro opening: X's second move must be at least {GameConfiguration.ProOpeningDistance} cells away from the centre.",
            _ => null
        };
    }
}
