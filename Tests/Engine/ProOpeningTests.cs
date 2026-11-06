using GameEngine;

namespace Tests.Engine;

public class ProOpeningTests
{
    private static readonly GameConfiguration Pro9 = TestBoards.FreeStyle9 with { Name = "Test pro", ProOpening = true };

    [Fact]
    public void EmptyBoard_OnlyTheCentreIsLegal()
    {
        var brain = new GameBrain(GameState.New(Pro9));

        Assert.Equal([new Move(4, 4)], brain.GetLegalMoves());
    }

    [Fact]
    public void FirstMoveOutsideCentre_IsRejected()
    {
        var brain = new GameBrain(GameState.New(Pro9));

        Assert.False(brain.TryMakeMove(0, 0, out var error));
        Assert.Contains("centre", error);
    }

    [Fact]
    public void FirstMoveOutsideCentre_WithoutProOpening_IsAllowed()
    {
        var brain = new GameBrain(GameState.New(TestBoards.FreeStyle9));

        Assert.True(brain.TryMakeMove(0, 0, out _));
    }

    [Fact]
    public void SecondPlayersFirstMove_CanBeNextToTheCentre()
    {
        var brain = new GameBrain(GameState.New(Pro9));
        brain.MakeMove(4, 4);

        Assert.True(brain.IsLegalMove(4, 5));
    }

    [Theory]
    [InlineData(4, 6, false)] // 2 cells to the right
    [InlineData(6, 6, false)] // 2 cells diagonally
    [InlineData(4, 7, true)]  // 3 cells to the right
    [InlineData(1, 1, true)]  // 3 cells diagonally
    [InlineData(8, 4, true)]  // 4 cells down
    public void FirstPlayersSecondMove_MustBeAtLeastThreeAwayFromCentre(int row, int col, bool expectedLegal)
    {
        var brain = new GameBrain(GameState.New(Pro9));
        brain.MakeMove(4, 4); // X in the centre
        brain.MakeMove(0, 0); // O anywhere

        Assert.Equal(expectedLegal, brain.IsLegalMove(row, col));
    }

    [Fact]
    public void EvenBoard_CentreIsTheLowerRightMiddleCell()
    {
        var config = Pro9 with { BoardWidth = 10, BoardHeight = 10 };
        var brain = new GameBrain(GameState.New(config));

        Assert.Equal(new Move(5, 5), brain.Centre);
        Assert.True(brain.IsLegalMove(5, 5));
    }
}
