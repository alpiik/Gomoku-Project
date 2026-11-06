using GameEngine;

namespace Tests.Engine;

public class GameBrainTests
{
    [Fact]
    public void NewGame_IsEmptyWithXToMove()
    {
        var brain = new GameBrain(GameState.New(TestBoards.FreeStyle9));

        Assert.Equal(EGameStatus.InProgress, brain.Status);
        Assert.Equal(EGamePiece.X, brain.State.NextMoveBy);
        Assert.Empty(brain.State.Moves);
        Assert.Equal(9 * 9, brain.GetLegalMoves().Count);
    }

    [Fact]
    public void NewGame_RectangularBoard_HasHeightRowsAndWidthColumns()
    {
        var config = new GameConfiguration("Rectangle", BoardWidth: 15, BoardHeight: 9, WinLength: 5);

        var brain = new GameBrain(GameState.New(config));

        Assert.Equal(9, brain.State.Board.Length);
        Assert.All(brain.State.Board, row => Assert.Equal(15, row.Length));
        Assert.Equal(15 * 9, brain.GetLegalMoves().Count);
    }

    [Fact]
    public void MakeMove_OnEmptyCell_PlacesPieceRecordsMoveAndSwitchesTurn()
    {
        var brain = new GameBrain(GameState.New(TestBoards.FreeStyle9));

        brain.MakeMove(4, 4);

        Assert.Equal(EGamePiece.X, brain.GetPiece(4, 4));
        Assert.Equal([new Move(4, 4)], brain.State.Moves);
        Assert.Equal(EGamePiece.O, brain.State.NextMoveBy);
    }

    [Fact]
    public void MakeMove_OnOccupiedCell_ThrowsInvalidMoveException()
    {
        var brain = new GameBrain(GameState.New(TestBoards.FreeStyle9));
        brain.MakeMove(4, 4);

        var ex = Assert.Throws<InvalidMoveException>(() => brain.MakeMove(4, 4));

        Assert.Equal(4, ex.Row);
        Assert.Equal(4, ex.Col);
    }

    [Theory]
    [InlineData(-1, 0)]
    [InlineData(0, -1)]
    [InlineData(9, 0)]
    [InlineData(0, 9)]
    public void MakeMove_OutsideBoard_ThrowsInvalidMoveException(int row, int col)
    {
        var brain = new GameBrain(GameState.New(TestBoards.FreeStyle9));

        Assert.Throws<InvalidMoveException>(() => brain.MakeMove(row, col));
    }

    [Fact]
    public void TryMakeMove_OnOccupiedCell_ReturnsFalseAndKeepsTheTurn()
    {
        var brain = new GameBrain(GameState.New(TestBoards.FreeStyle9));
        brain.MakeMove(4, 4);

        var ok = brain.TryMakeMove(4, 4, out var error);

        Assert.False(ok);
        Assert.NotNull(error);
        Assert.Equal(EGamePiece.O, brain.State.NextMoveBy);
        Assert.Single(brain.State.Moves);
    }

    [Fact]
    public void MakeMove_AfterTheGameIsWon_ThrowsInvalidMoveException()
    {
        var brain = new GameBrain(TestBoards.FromPicture(TestBoards.FreeStyle9, "XXXX"));
        brain.MakeMove(0, 4);

        Assert.Throws<InvalidMoveException>(() => brain.MakeMove(8, 8));
        Assert.Empty(brain.GetLegalMoves());
    }

    [Fact]
    public void GetLegalMoves_NeverContainsOccupiedCells()
    {
        var brain = new GameBrain(TestBoards.FromPicture(TestBoards.FreeStyle9, """
            X.O
            .X.
            """));

        var moves = brain.GetLegalMoves();

        Assert.Equal(9 * 9 - 3, moves.Count);
        Assert.DoesNotContain(new Move(0, 0), moves);
        Assert.DoesNotContain(new Move(1, 1), moves);
    }

    [Fact]
    public void MakeMove_FillingTheLastCellWithoutALine_IsADraw()
    {
        // no four in a row anywhere; the bottom-right cell is still empty and X plays it
        var config = new GameConfiguration("Draw", 9, 9, 4);
        var brain = new GameBrain(TestBoards.FromPicture(config, """
            XXOOXXOOX
            OOXXOOXXO
            XXOOXXOOX
            OOXXOOXXO
            XXOOXXOOX
            OOXXOOXXO
            XXOOXXOOX
            OOXXOOXXO
            XXOOXXOO.
            """));

        var status = brain.MakeMove(8, 8);

        Assert.Equal(EGameStatus.Draw, status);
        Assert.True(brain.IsDraw());
        Assert.Empty(brain.GetLegalMoves());
    }

    [Fact]
    public void Constructor_InvalidConfiguration_ThrowsArgumentException()
    {
        var config = new GameConfiguration("Too small", 3, 3, 3);

        Assert.Throws<ArgumentException>(() => new GameBrain(GameState.New(config)));
    }
}
