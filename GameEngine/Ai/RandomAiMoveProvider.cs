namespace GameEngine.Ai;

/// <summary>
/// "Level 0": a uniformly random legal move. The <see cref="Random"/> is passed in: the program shares one
/// for the whole run (never a new one per move), and a test with a fixed seed always gets the same moves.
/// </summary>
public class RandomAiMoveProvider(Random random) : IMoveProvider
{
    public Task<Move?> GetMoveAsync(GameState state, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        // GetLegalMoves only reads the state; pro opening rules are already applied in it
        var legal = new GameBrain(state).GetLegalMoves();
        if (legal.Count == 0)
        {
            throw new InvalidOperationException("No legal moves: the game is already over.");
        }

        return Task.FromResult<Move?>(legal[random.Next(legal.Count)]);
    }
}
