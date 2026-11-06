using GameEngine;

namespace Tests.Engine;

public class WinDetectionTests
{
    [Theory]
    [InlineData("XXXX", 0, 4, "horizontal, played at the end")]
    [InlineData("XX.XX", 0, 2, "horizontal, played in the middle")]
    [InlineData("X\nX\nX\nX", 4, 0, "vertical")]
    [InlineData("X\n.X\n..X\n...X", 4, 4, "diagonal down-right")]
    [InlineData("....X\n...X\n..X\n.X", 4, 0, "diagonal down-left")]
    public void MakeMove_CompletingFive_XWins(string picture, int row, int col, string direction)
    {
        var brain = new GameBrain(TestBoards.FromPicture(TestBoards.FreeStyle9, picture));

        var status = brain.MakeMove(row, col);

        Assert.True(status == EGameStatus.XWon, $"{direction} line was not detected");
    }

    [Fact]
    public void MakeMove_CompletingFiveWithO_OWins()
    {
        var brain = new GameBrain(TestBoards.FromPicture(TestBoards.FreeStyle9, "OOOO", EGamePiece.O));

        Assert.Equal(EGameStatus.OWon, brain.MakeMove(0, 4));
    }

    [Theory]
    [InlineData("XXX", 0, 3)]           // only four
    [InlineData("XXXO", 0, 4)]          // interrupted by O
    [InlineData("XX\n..X\n...X", 4, 4)] // not on one line
    public void MakeMove_NoFiveInALine_GameContinues(string picture, int row, int col)
    {
        var brain = new GameBrain(TestBoards.FromPicture(TestBoards.FreeStyle9, picture));

        Assert.Equal(EGameStatus.InProgress, brain.MakeMove(row, col));
    }

    [Fact]
    public void MakeMove_LineEndingInBottomRightCornerOfRectangle_XWins()
    {
        // 9 rows x 15 columns: catches a row/column swap and an off-by-one at the edge
        var config = new GameConfiguration("Rectangle", BoardWidth: 15, BoardHeight: 9, WinLength: 5);
        var brain = new GameBrain(TestBoards.FromPicture(config, """
            ...............
            ...............
            ...............
            ...............
            ...............
            ...............
            ...............
            ...............
            ..........XXXX.
            """));

        Assert.Equal(EGameStatus.XWon, brain.MakeMove(8, 14));
    }

    [Fact]
    public void MakeMove_VerticalLineEndingInBottomRowOfRectangle_XWins()
    {
        var config = new GameConfiguration("Rectangle", BoardWidth: 15, BoardHeight: 9, WinLength: 5);
        var brain = new GameBrain(TestBoards.FromPicture(config, """
            ...............
            ...............
            ...............
            ...............
            ..............X
            ..............X
            ..............X
            ..............X
            ...............
            """));

        Assert.Equal(EGameStatus.XWon, brain.MakeMove(8, 14));
    }

    [Theory]
    [InlineData(4, "XXX", 0, 3, EGameStatus.XWon)]
    [InlineData(6, "XXXXX", 0, 5, EGameStatus.XWon)]
    [InlineData(6, "XXXX", 0, 4, EGameStatus.InProgress)]
    public void MakeMove_UsesConfiguredWinLength(int winLength, string picture, int row, int col, EGameStatus expected)
    {
        var config = TestBoards.FreeStyle9 with { WinLength = winLength };
        var brain = new GameBrain(TestBoards.FromPicture(config, picture));

        Assert.Equal(expected, brain.MakeMove(row, col));
    }

    [Theory]
    [InlineData(false, EGameStatus.XWon)]       // free-style: six or more also wins
    [InlineData(true, EGameStatus.InProgress)]  // standard: an overline is not a win
    public void MakeMove_SixInALine_DependsOnOverlineRule(bool exactWinLength, EGameStatus expected)
    {
        var config = TestBoards.FreeStyle9 with { ExactWinLength = exactWinLength };
        var brain = new GameBrain(TestBoards.FromPicture(config, "XXX.XX"));

        Assert.Equal(expected, brain.MakeMove(0, 3));
    }

    [Fact]
    public void MakeMove_ExactlyFiveWithOverlineRule_XWins()
    {
        var brain = new GameBrain(TestBoards.FromPicture(TestBoards.Exact9, "XX.XX"));

        Assert.Equal(EGameStatus.XWon, brain.MakeMove(0, 2));
    }

    [Fact]
    public void MakeMove_OverlineInOneDirectionButExactlyFiveInAnother_XWins()
    {
        // the move at (0,3) makes six horizontally (not a win) and exactly five vertically (a win)
        var brain = new GameBrain(TestBoards.FromPicture(TestBoards.Exact9, """
            XXX.XX
            ...X
            ...X
            ...X
            ...X
            """));

        Assert.Equal(EGameStatus.XWon, brain.MakeMove(0, 3));
    }
}
