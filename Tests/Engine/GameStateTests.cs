using GameEngine;

namespace Tests.Engine;

public class GameStateTests
{
    [Fact]
    public void New_WithoutName_IsNamedAfterConfiguration()
    {
        Assert.Equal("Small", GameState.New(Presets.Small).Name);
        Assert.Equal("Evening game", GameState.New(Presets.Small, "  Evening game ").Name);
    }

    [Fact]
    public void Validate_NewGame_HasNoProblems()
    {
        Assert.Empty(GameState.New(Presets.Pro).Validate());
    }

    // states a hand-edited save file could contain
    public static TheoryData<string> DamagedStates => ["board too small", "row too short", "nobody to move", "move outside"];

    [Theory]
    [MemberData(nameof(DamagedStates))]
    public void Validate_DamagedState_ReturnsProblem_AndGameBrainRefusesIt(string damage)
    {
        var state = GameState.New(Presets.Small);
        switch (damage)
        {
            case "board too small":
                state.Board = GameState.CreateBoard(3, 3);
                break;
            case "row too short":
                state.Board[4] = new EGamePiece[2];
                break;
            case "nobody to move":
                state.NextMoveBy = EGamePiece.Empty;
                break;
            case "move outside":
                state.Moves.Add(new Move(9, 0));
                break;
        }

        Assert.NotEmpty(state.Validate());
        Assert.Throws<ArgumentException>(() => new GameBrain(state));
    }
}
