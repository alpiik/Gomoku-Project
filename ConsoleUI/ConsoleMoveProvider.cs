using GameEngine;

namespace ConsoleUI;

/// <summary>A human at the keyboard. Reads "row,col" with 1-based numbers, as they are shown on the board.</summary>
public class ConsoleMoveProvider : IMoveProvider
{
    public Move? GetMove(GameState state)
    {
        while (true)
        {
            Console.Write($"Player {state.NextMoveBy}, your move as row,col (Q to leave the game): ");
            var input = Console.ReadLine();
            if (input is null || input.Trim().Equals("Q", StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            if (TryParseMove(input, out var move))
            {
                return move;
            }

            Console.WriteLine("Type the row and the column, for example 8,8");
        }
    }

    // the player types 1-based numbers, the engine works with 0-based ones
    private static bool TryParseMove(string input, out Move move)
    {
        move = default;
        var parts = input.Split([',', ' '], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (parts.Length != 2 || !int.TryParse(parts[0], out var row) || !int.TryParse(parts[1], out var col))
        {
            return false;
        }

        move = new Move(row - 1, col - 1);
        return true;
    }
}
