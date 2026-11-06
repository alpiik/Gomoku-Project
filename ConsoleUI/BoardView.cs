using GameEngine;

namespace ConsoleUI;

/// <summary>
/// Draws the board with 1-based row numbers on both sides and column numbers above and below.
/// Only draws: it never decides whether a move is legal or who has won.
/// </summary>
public static class BoardView
{
    public static void Draw(GameBrain brain, string message)
    {
        if (!Console.IsOutputRedirected)
        {
            Console.Clear();
        }

        var config = brain.Config;
        Console.WriteLine($"Gomoku - {config.Name}: {Describe(config)}");
        Console.WriteLine();

        Move? lastMove = brain.State.Moves.Count > 0 ? brain.State.Moves[^1] : null;
        WriteColumnNumbers(config.BoardWidth);
        for (var row = 0; row < config.BoardHeight; row++)
        {
            Console.Write($"{row + 1,3} ");
            for (var col = 0; col < config.BoardWidth; col++)
            {
                WriteCell(brain.GetPiece(row, col), lastMove == new Move(row, col));
            }

            Console.WriteLine($" {row + 1}");
        }

        WriteColumnNumbers(config.BoardWidth);
        Console.WriteLine();

        if (lastMove is { } last)
        {
            Console.WriteLine($"Last move: {brain.GetPiece(last.Row, last.Col)} at {last.Row + 1},{last.Col + 1} (shown as [ ])");
        }

        if (brain.Status == EGameStatus.InProgress)
        {
            Console.WriteLine($"Move {brain.State.Moves.Count + 1}. Player {brain.State.NextMoveBy} to move.");
        }

        if (message != "")
        {
            Console.WriteLine(message);
        }
    }

    public static string Describe(GameConfiguration config)
    {
        var winRule = config.ExactWinLength
            ? $"exactly {config.WinLength} wins"
            : $"{config.WinLength} or more wins";
        var proOpening = config.ProOpening ? ", pro opening" : "";
        return $"{config.BoardWidth} columns x {config.BoardHeight} rows, {winRule}{proOpening}";
    }

    private static void WriteColumnNumbers(int width)
    {
        Console.Write("    ");
        for (var col = 0; col < width; col++)
        {
            Console.Write($"{col + 1,2} ");
        }

        Console.WriteLine();
    }

    // every cell is three characters wide; the last move gets brackets so it shows even without colours
    private static void WriteCell(EGamePiece piece, bool isLastMove)
    {
        var symbol = piece switch
        {
            EGamePiece.X => "X",
            EGamePiece.O => "O",
            _ => "."
        };

        Console.ForegroundColor = piece switch
        {
            EGamePiece.X => ConsoleColor.Red,
            EGamePiece.O => ConsoleColor.Cyan,
            _ => ConsoleColor.DarkGray
        };
        Console.Write(isLastMove ? $"[{symbol}]" : $" {symbol} ");
        Console.ResetColor();
    }
}
