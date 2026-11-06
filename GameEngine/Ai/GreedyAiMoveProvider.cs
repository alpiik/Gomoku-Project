namespace GameEngine.Ai;

/// <summary>
/// Bonus "greedy random" player: takes an immediate win when one exists, otherwise a random legal move.
/// <see cref="GameBrain.WouldWin"/> checks each move without placing it, so the real state never changes.
/// </summary>
public class GreedyAiMoveProvider(Random random) : IMoveProvider
{
    public Task<Move?> GetMoveAsync(GameState state, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var brain = new GameBrain(state);
        var legal = brain.GetLegalMoves();
        if (legal.Count == 0)
        {
            throw new InvalidOperationException("No legal moves: the game is already over.");
        }

        foreach (var move in legal)
        {
            if (brain.WouldWin(move.Row, move.Col))
            {
                return Task.FromResult<Move?>(move);
            }
        }

        return Task.FromResult<Move?>(legal[random.Next(legal.Count)]);
    }
}
