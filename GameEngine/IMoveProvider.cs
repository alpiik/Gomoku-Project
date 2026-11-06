namespace GameEngine;

/// <summary>
/// Whoever chooses the next move: a human at the console now, a random AI in A5, minimax in A6.
/// The game loop only talks to this interface, so it does not care who is playing.
/// </summary>
public interface IMoveProvider
{
    /// <summary>
    /// Returns the move for <see cref="GameState.NextMoveBy"/>, or null when the player wants to leave the game.
    /// Must not change the state; the game loop makes the move.
    /// </summary>
    Move? GetMove(GameState state);
}
