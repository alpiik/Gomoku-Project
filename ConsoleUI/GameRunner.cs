using GameEngine;
using MenuSystem;

namespace ConsoleUI;

/// <summary>
/// The game loop: draw, ask the current player's move provider, let the brain make the move, repeat.
/// The same loop works for any provider, so AI players (A5, A6) need no changes here.
/// </summary>
public static class GameRunner
{
    public static string Run(GameState state, IMoveProvider xPlayer, IMoveProvider oPlayer)
    {
        var brain = new GameBrain(state);
        var message = "";

        while (brain.Status == EGameStatus.InProgress)
        {
            BoardView.Draw(brain, message);
            var provider = state.NextMoveBy == EGamePiece.X ? xPlayer : oPlayer;
            var move = provider.GetMove(state);
            if (move is null)
            {
                return MenuResult.None; // the player left the game
            }

            // an illegal move only shows a message; the same player is asked again
            message = brain.TryMakeMove(move.Value.Row, move.Value.Col, out var error)
                ? ""
                : $"Illegal move: {error}";
        }

        BoardView.Draw(brain, ResultText(brain.Status));
        ConsoleInput.Pause();
        return MenuResult.None;
    }

    private static string ResultText(EGameStatus status) => status switch
    {
        EGameStatus.XWon => "Player X wins!",
        EGameStatus.OWon => "Player O wins!",
        _ => "Draw - the board is full."
    };
}
