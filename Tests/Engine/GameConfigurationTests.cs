using GameEngine;

namespace Tests.Engine;

public class GameConfigurationTests
{
    [Fact]
    public void Presets_AreAllValid()
    {
        Assert.All(Presets.All, preset => Assert.Empty(preset.Validate()));
    }

    [Theory]
    [InlineData(9, 9, 4)]
    [InlineData(19, 19, 6)]
    [InlineData(9, 19, 5)]
    public void Validate_BoundaryValues_AreValid(int width, int height, int winLength)
    {
        var config = new GameConfiguration("Edge", width, height, winLength);

        Assert.Empty(config.Validate());
    }

    public static TheoryData<GameConfiguration> InvalidConfigurations => new()
    {
        new GameConfiguration("", 15, 15, 5),       // no name
        new GameConfiguration("   ", 15, 15, 5),    // whitespace name
        new GameConfiguration("Narrow", 8, 15, 5),  // width too small
        new GameConfiguration("Wide", 20, 15, 5),   // width too big
        new GameConfiguration("Flat", 15, 8, 5),    // height too small
        new GameConfiguration("Short", 15, 15, 3),  // win length too small
        new GameConfiguration("Long", 15, 15, 7),   // win length too big
        new GameConfiguration(new string('a', 65), 15, 15, 5), // name too long
        new GameConfiguration("../escape", 15, 15, 5),          // could point outside the save folder
        new GameConfiguration("a:b", 15, 15, 5),                // not allowed in a Windows file name
        new GameConfiguration(" padded", 15, 15, 5),            // space at the start
    };

    [Theory]
    [InlineData("Free-style")]
    [InlineData("my board_2")]
    [InlineData("Õun 19x19")]
    public void IsValidName_LettersDigitsSpacesDashesUnderscores_AreAllowed(string name)
    {
        Assert.True(GameConfiguration.IsValidName(name));
    }

    [Theory]
    [MemberData(nameof(InvalidConfigurations))]
    public void Validate_InvalidConfiguration_ReturnsErrors(GameConfiguration config)
    {
        Assert.NotEmpty(config.Validate());
    }

    [Fact]
    public void Validate_SeveralProblems_ReturnsAllOfThem()
    {
        var config = new GameConfiguration("", 2, 2, 9);

        Assert.Equal(4, config.Validate().Count);
    }

    [Theory]
    [InlineData("Standard")]
    [InlineData("standard")]
    [InlineData(" Pro ")]
    public void IsPreset_IgnoresCaseAndSpaces(string name)
    {
        Assert.True(Presets.IsPreset(name));
    }
}
