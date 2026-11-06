using GameEngine;
using GameEngine.Ai;
using Tests.Engine;

namespace Tests.Ai;

public class RandomAiMoveProviderTests
{
    [Theory]
    [MemberData(nameof(AiGames.PresetsAndSeeds), MemberType = typeof(AiGames))]
    public async Task AiVsAi_PlaysOnlyLegalMoves_AndEnds(GameConfiguration config, int seed)
    {
        var random = new Random(seed);

        // PlayToTheEndAsync checks every move against GetLegalMoves and fails if the game doesn't end
        await AiGames.PlayToTheEndAsync(config, new RandomAiMoveProvider(random), new RandomAiMoveProvider(random));
    }

    [Fact]
    public async Task SameSeed_PlaysTheSameGame()
    {
        var first = await PlayWithSeed(42);
        var second = await PlayWithSeed(42);

        Assert.Equal(first.Moves, second.Moves);
    }

    [Fact]
    public async Task DifferentSeeds_PlayDifferentGames()
    {
        var first = await PlayWithSeed(1);
        var second = await PlayWithSeed(2);

        Assert.NotEqual(first.Moves, second.Moves);
    }

    [Fact]
    public async Task GetMove_DoesNotChangeTheState()
    {
        var state = GameState.New(Presets.Standard);
        new GameBrain(state).MakeMove(7, 7);
        var before = GameStateJson.ToJson(state);

        await new RandomAiMoveProvider(new Random(1)).GetMoveAsync(state, CancellationToken.None);

        Assert.Equal(before, GameStateJson.ToJson(state));
    }

    [Fact]
    public async Task ProOpening_FirstMoveIsTheCentre()
    {
        var state = GameState.New(Presets.Pro);

        var move = await new RandomAiMoveProvider(new Random(3)).GetMoveAsync(state, CancellationToken.None);

        Assert.Equal(new GameBrain(state).Centre, move);
    }

    [Fact]
    public async Task OnlyOneCellLeft_PlaysIt()
    {
        // 9x9 board full except the bottom-right corner, no line of 5 anywhere
        var rows = Enumerable.Range(0, 9).Select(row => row % 4 < 2 ? "XOXOXOXOX" : "OXOXOXOXO").ToArray();
        rows[8] = rows[8][..8] + ".";
        var state = TestBoards.FromPicture(TestBoards.FreeStyle9, string.Join('\n', rows));

        var move = await new RandomAiMoveProvider(new Random(5)).GetMoveAsync(state, CancellationToken.None);

        Assert.Equal(new Move(8, 8), move);
    }

    [Fact]
    public async Task CancelledToken_Throws()
    {
        var provider = new RandomAiMoveProvider(new Random(1));
        var cancelled = new CancellationToken(canceled: true);

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => provider.GetMoveAsync(GameState.New(Presets.Small), cancelled));
    }

    [Fact]
    public async Task FinishedGame_Throws()
    {
        var state = TestBoards.FromPicture(Presets.Small, "XXX.");
        new GameBrain(state).MakeMove(0, 3); // X wins
        var provider = new RandomAiMoveProvider(new Random(1));

        await Assert.ThrowsAsync<InvalidOperationException>(() => provider.GetMoveAsync(state, CancellationToken.None));
    }

    private static Task<GameState> PlayWithSeed(int seed)
    {
        var random = new Random(seed);
        return AiGames.PlayToTheEndAsync(Presets.Standard,
            new RandomAiMoveProvider(random), new RandomAiMoveProvider(random));
    }
}
