using GameEngine;
using MenuSystem;

namespace ConsoleUI;

/// <summary>
/// The game loop: draw, ask the current player's move provider, let the brain make the move, save, repeat.
/// It never asks what kind of player is playing: humans, random AI and (A6) minimax all go through
/// <see cref="IMoveProvider"/>, so new players need no changes here.
/// </summary>
public static class GameRunner
{
    // how often a waiting loop looks at the keyboard
    private static readonly TimeSpan KeyCheckInterval = TimeSpan.FromMilliseconds(50);

    /// <summary>
    /// Plays a new or a loaded game. The game is saved after every move, so leaving or stopping never loses anything.
    /// </summary>
    /// <param name="pauseBeforeMove">
    /// Waits this long after drawing the board, so AI vs AI can be watched. The caller decides how long;
    /// zero when a human plays.
    /// </param>
    public static async Task<string> RunAsync(GameState state, IMoveProvider xPlayer, IMoveProvider oPlayer,
        IGameRepository games, TimeSpan pauseBeforeMove)
    {
        var brain = new GameBrain(state);
        var message = "";

        // a key press while an AI thinks or while the loop pauses cancels this; the AI and the pause then stop
        using var stop = new CancellationTokenSource();

        try
        {
            while (brain.Status == EGameStatus.InProgress)
            {
                BoardView.Draw(brain, message);
                if (pauseBeforeMove > TimeSpan.Zero)
                {
                    await WatchStopKeyAsync(Task.Delay(pauseBeforeMove, stop.Token), stop);
                }

                var provider = state.NextMoveBy == EGamePiece.X ? xPlayer : oPlayer;
                var moveTask = provider.GetMoveAsync(state, stop.Token);
                await WatchStopKeyAsync(moveTask, stop);
                var move = await moveTask;
                if (move is null)
                {
                    return MenuResult.None; // the player left the game
                }

                // an illegal move only shows a message; the same player is asked again
                if (brain.TryMakeMove(move.Value.Row, move.Value.Col, out var error))
                {
                    games.Save(state); // autosave
                    message = "";
                }
                else
                {
                    message = $"Illegal move: {error}";
                }
            }
        }
        catch (OperationCanceledException)
        {
            // the last finished move is already saved; the move that was being thought about is simply dropped
            BoardView.Draw(brain, "Stopped. The game is saved: load it from the main menu to continue.");
            ConsoleInput.Pause();
            return MenuResult.None;
        }

        BoardView.Draw(brain, ResultText(brain.Status));
        ConsoleInput.Pause();
        return MenuResult.None;
    }

    /// <summary>
    /// Waits for the task while watching the keyboard: any key cancels <paramref name="stop"/>, and the task
    /// (which got the token) then ends with <see cref="OperationCanceledException"/>, rethrown here.
    /// A human's move task is already finished when it arrives, so this never takes a human's keys.
    /// </summary>
    private static async Task WatchStopKeyAsync(Task task, CancellationTokenSource stop)
    {
        while (!task.IsCompleted)
        {
            if (KeyPressed())
            {
                Console.ReadKey(intercept: true); // take the key, so it doesn't end up in the next prompt
                stop.Cancel();
            }

            await Task.WhenAny(task, Task.Delay(KeyCheckInterval));
        }

        await task;
    }

    // with redirected input (piped test scripts) there is no keyboard to watch
    private static bool KeyPressed() => !Console.IsInputRedirected && Console.KeyAvailable;

    private static string ResultText(EGameStatus status) => status switch
    {
        EGameStatus.XWon => "Player X wins!",
        EGameStatus.OWon => "Player O wins!",
        _ => "Draw - the board is full."
    };
}
