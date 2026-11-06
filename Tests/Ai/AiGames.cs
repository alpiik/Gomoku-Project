using GameEngine;

namespace Tests.Ai;

public static class AiGames
{
    /// <summary>
    /// Plays a whole game the way the console's game loop does (ask the provider, then let the brain make
    /// the move), but checks every move before making it. Stops after width x height moves at the latest:
    /// a game that is still running then would never end.
    /// </summary>
    public static async Task<GameState> PlayToTheEndAsync(GameConfiguration config, IMoveProvider x, IMoveProvider o)
    {
        var state = GameState.New(config);
        var brain = new GameBrain(state);
        var maxMoves = config.BoardWidth * config.BoardHeight;

        for (var turn = 0; turn < maxMoves && brain.Status == EGameStatus.InProgress; turn++)
        {
            var provider = state.NextMoveBy == EGamePiece.X ? x : o;
            var move = await provider.GetMoveAsync(state, CancellationToken.None);

            Assert.NotNull(move);
            Assert.Contains(move.Value, brain.GetLegalMoves());
            brain.MakeMove(move.Value.Row, move.Value.Col);
        }

        Assert.NotEqual(EGameStatus.InProgress, brain.Status);
        return state;
    }

    /// <summary>Every preset (exact win length, pro opening, 9x9 up to 19x19) with a few fixed seeds.</summary>
    public static TheoryData<GameConfiguration, int> PresetsAndSeeds()
    {
        var data = new TheoryData<GameConfiguration, int>();
        foreach (var config in Presets.All)
        {
            foreach (var seed in new[] { 1, 7, 42, 2026 })
            {
                data.Add(config, seed);
            }
        }

        return data;
    }
}
