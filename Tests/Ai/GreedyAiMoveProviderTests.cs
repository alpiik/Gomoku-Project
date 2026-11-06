using GameEngine;
using GameEngine.Ai;
using Tests.Engine;

namespace Tests.Ai;

public class GreedyAiMoveProviderTests
{
    [Theory]
    [MemberData(nameof(AiGames.PresetsAndSeeds), MemberType = typeof(AiGames))]
    public async Task AiVsAi_PlaysOnlyLegalMoves_AndEnds(GameConfiguration config, int seed)
    {
        var random = new Random(seed);

        await AiGames.PlayToTheEndAsync(config, new GreedyAiMoveProvider(random), new RandomAiMoveProvider(random));
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    public async Task TakesTheWinningMove(int seed)
    {
        // free-style 5: X has four in the top row
        var state = TestBoards.FromPicture(TestBoards.FreeStyle9, """
            XXXX.
            OOO..
            O....
            """);

        var move = await new GreedyAiMoveProvider(new Random(seed)).GetMoveAsync(state, CancellationToken.None);

        Assert.Equal(new Move(0, 4), move);
    }

    [Fact]
    public async Task TakesTheWinningMove_ForO()
    {
        var state = TestBoards.FromPicture(TestBoards.FreeStyle9, """
            OOOO.
            XXX..
            X...X
            """, nextMoveBy: EGamePiece.O);

        var move = await new GreedyAiMoveProvider(new Random(1)).GetMoveAsync(state, CancellationToken.None);

        Assert.Equal(new Move(0, 4), move);
    }

    [Fact]
    public async Task ExactRule_SixInARowIsNotAWin_SoTheRealWinIsChosen()
    {
        // exactly 5: (0,2) would make six X in a row, which doesn't win; (2,4) makes exactly five
        var state = TestBoards.FromPicture(TestBoards.Exact9, """
            XX.XXX
            ......
            XXXX..
            OOO...
            OOO...
            """);

        var move = await new GreedyAiMoveProvider(new Random(1)).GetMoveAsync(state, CancellationToken.None);

        Assert.Equal(new Move(2, 4), move);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(99)]
    public async Task WithoutAWinningMove_PlaysLikeRandom(int seed)
    {
        var state = GameState.New(Presets.Standard);
        new GameBrain(state).MakeMove(7, 7);

        var greedy = await new GreedyAiMoveProvider(new Random(seed)).GetMoveAsync(state, CancellationToken.None);
        var random = await new RandomAiMoveProvider(new Random(seed)).GetMoveAsync(state, CancellationToken.None);

        Assert.Equal(random, greedy);
    }

    [Fact]
    public async Task GetMove_DoesNotChangeTheState()
    {
        // a winning move exists, so WouldWin is asked about many cells
        var state = TestBoards.FromPicture(TestBoards.FreeStyle9, "XXXX.\nOOO..");
        var before = GameStateJson.ToJson(state);

        await new GreedyAiMoveProvider(new Random(1)).GetMoveAsync(state, CancellationToken.None);

        Assert.Equal(before, GameStateJson.ToJson(state));
    }

    [Fact]
    public async Task CancelledToken_Throws()
    {
        var provider = new GreedyAiMoveProvider(new Random(1));

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => provider.GetMoveAsync(GameState.New(Presets.Small), new CancellationToken(canceled: true)));
    }
}
