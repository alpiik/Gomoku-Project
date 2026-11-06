using System.Text.Json;
using GameEngine;

namespace Tests.Engine;

/// <summary>The shared JSON format that both the files and the database use.</summary>
public class GameStateJsonTests
{
    private const string SmallConfig = """{ "Name": "Small", "BoardWidth": 9, "BoardHeight": 9, "WinLength": 4 }""";

    [Fact]
    public void ToJson_StoresEnumsAsText_AndTheFormatVersion()
    {
        var state = GameState.New(Presets.Small);
        new GameBrain(state).MakeMove(4, 4);

        var json = GameStateJson.ToJson(state);

        Assert.Contains("\"NextMoveBy\": \"O\"", json);
        Assert.Contains("\"X\"", json);
        Assert.Contains($"\"Version\": {GameStateJson.CurrentVersion}", json);
    }

    [Fact]
    public void ToJsonThenFromJson_GivesAnEqualState()
    {
        var state = GameState.New(Presets.Pro, "Text round trip");
        new GameBrain(state).MakeMove(9, 9);

        var loaded = GameStateJson.FromJson(GameStateJson.ToJson(state), "test");

        Assert.Equal(state.Id, loaded.Id);
        Assert.Equal(state.Config, loaded.Config);
        Assert.Equal(state.Board, loaded.Board);
        Assert.Equal(state.Moves, loaded.Moves);
    }

    [Fact]
    public void PlayerTypes_AreStoredAsText_AndReadBack()
    {
        var state = GameState.New(Presets.Small, "Players", EPlayerType.RandomAi, EPlayerType.GreedyAi);

        var json = GameStateJson.ToJson(state);
        var loaded = GameStateJson.FromJson(json, "test");

        Assert.Contains("\"PlayerX\": \"RandomAi\"", json);
        Assert.Equal(EPlayerType.RandomAi, loaded.PlayerX);
        Assert.Equal(EPlayerType.GreedyAi, loaded.PlayerO);
    }

    [Fact]
    public void Version1Text_FromBeforeA5_LoadsAsHumanVsHuman()
    {
        var state = GameState.New(Presets.Small, "Saved by A4");
        new GameBrain(state).MakeMove(4, 4);

        var loaded = GameStateJson.FromJson(OldSaves.Version1Json(state), "old file");

        Assert.Equal(EPlayerType.Human, loaded.PlayerX);
        Assert.Equal(EPlayerType.Human, loaded.PlayerO);
        Assert.Equal(state.Moves, loaded.Moves);
    }

    [Theory]
    [InlineData("{ \"Version\": 99, \"State\": { \"Config\": " + SmallConfig + " } }")] // newer program
    [InlineData("{ \"State\": { \"Config\": " + SmallConfig + " } }")]                  // no version at all
    public void FromJson_UnknownVersion_ThrowsNotSupported(string json)
    {
        Assert.Throws<NotSupportedException>(() => GameStateJson.FromJson(json, "test"));
    }

    [Theory]
    [InlineData("{ \"Version\": 1, \"State\": ")]                                                      // broken text
    [InlineData("{ \"Version\": 1, \"State\": {} }")]                                                  // no configuration
    [InlineData("{ \"Version\": 1, \"State\": { \"Config\": " + SmallConfig + ", \"Board\": null } }")] // null board
    [InlineData("{ \"Version\": 1, \"State\": { \"Config\": " + SmallConfig + ", \"NextMoveBy\": 1 } }")] // enum as number
    [InlineData("{ \"Version\": 1, \"State\": { \"Config\": " + SmallConfig + " } }")]                // no board, unplayable
    public void FromJson_BrokenOrUnplayable_ThrowsJsonException(string json)
    {
        Assert.Throws<JsonException>(() => GameStateJson.FromJson(json, "test"));
    }
}
