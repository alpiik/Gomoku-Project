namespace GameEngine;

/// <summary>
/// Whoever chooses the next move: a human at the console, a random AI (A5), minimax (A6).
/// The game loop only talks to this interface, so it never needs to know who is playing.
/// </summary>
public interface IMoveProvider
{
    /// <summary>
    /// Returns the move for <see cref="GameState.NextMoveBy"/>, or null when the player wants to leave the game.
    /// Must not change the state; the game loop makes the move.
    /// It returns a <see cref="Task"/> so a slow AI (minimax in A6) can think without freezing the console,
    /// and it takes a token so the player can stop it; a cancelled token ends with
    /// <see cref="OperationCanceledException"/>.
    /// </summary>
    Task<Move?> GetMoveAsync(GameState state, CancellationToken cancellationToken);
}
