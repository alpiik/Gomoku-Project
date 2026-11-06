using GameEngine;

namespace Tests.Engine;

public static class TestBoards
{
    public static readonly GameConfiguration FreeStyle9 = new("Test free-style", 9, 9, 5);
    public static readonly GameConfiguration Exact9 = FreeStyle9 with { Name = "Test exact", ExactWinLength = true };

    /// <summary>
    /// A new game whose board starts with the picture: rows separated by newlines, 'X' and 'O' are pieces,
    /// anything else is empty. The picture is drawn from the top-left corner and may be smaller than the board.
    /// </summary>
    public static GameState FromPicture(GameConfiguration config, string picture, EGamePiece nextMoveBy = EGamePiece.X)
    {
        var state = GameState.New(config);
        state.NextMoveBy = nextMoveBy;

        var rows = picture.Split('\n', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        for (var row = 0; row < rows.Length; row++)
        {
            for (var col = 0; col < rows[row].Length; col++)
            {
                state.Board[row][col] = rows[row][col] switch
                {
                    'X' => EGamePiece.X,
                    'O' => EGamePiece.O,
                    _ => EGamePiece.Empty
                };
            }
        }

        return state;
    }
}
