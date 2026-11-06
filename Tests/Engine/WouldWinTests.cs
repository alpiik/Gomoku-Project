using GameEngine;

namespace Tests.Engine;

/// <summary>GameBrain.WouldWin: "would this move win?" without making it.</summary>
public class WouldWinTests
{
    [Fact]
    public void CompletingALine_WouldWin_AndTheBoardStaysUnchanged()
    {
        var state = TestBoards.FromPicture(TestBoards.FreeStyle9, "XXXX.");
        var brain = new GameBrain(state);

        Assert.True(brain.WouldWin(0, 4));
        Assert.Equal(EGamePiece.Empty, brain.GetPiece(0, 4));
        Assert.Empty(state.Moves);
        Assert.Equal(EGamePiece.X, state.NextMoveBy);
    }

    [Fact]
    public void OnlyThePlayerToMoveCounts()
    {
        // O's four don't help X
        var brain = new GameBrain(TestBoards.FromPicture(TestBoards.FreeStyle9, "OOOO."));

        Assert.False(brain.WouldWin(0, 4));
    }

    [Fact]
    public void Overline_WinsInFreeStyle_ButNotWithTheExactRule()
    {
        const string sixWithTheGapFilled = "XX.XXX";

        Assert.True(new GameBrain(TestBoards.FromPicture(TestBoards.FreeStyle9, sixWithTheGapFilled)).WouldWin(0, 2));
        Assert.False(new GameBrain(TestBoards.FromPicture(TestBoards.Exact9, sixWithTheGapFilled)).WouldWin(0, 2));
    }

    [Theory]
    [InlineData(0, 0)]  // taken
    [InlineData(-1, 0)] // outside
    [InlineData(0, 9)]  // outside
    public void TakenOrOutsideCell_WouldNotWin(int row, int col)
    {
        var brain = new GameBrain(TestBoards.FromPicture(TestBoards.FreeStyle9, "XXXX."));

        Assert.False(brain.WouldWin(row, col));
    }
}
