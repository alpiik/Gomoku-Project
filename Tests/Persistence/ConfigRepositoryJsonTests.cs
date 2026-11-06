using System.Text.Json;
using DAL.Json;
using GameEngine;

namespace Tests.Persistence;

/// <summary>Every test gets its own empty temporary folder, deleted again afterwards (xUnit calls Dispose).</summary>
public class ConfigRepositoryJsonTests : IDisposable
{
    private readonly string _folder = Path.Combine(Path.GetTempPath(), "gomoku-tests", Guid.NewGuid().ToString());
    private readonly ConfigRepositoryJson _repository;

    public ConfigRepositoryJsonTests()
    {
        _repository = new ConfigRepositoryJson(_folder);
    }

    public void Dispose()
    {
        Directory.Delete(_folder, recursive: true);
    }

    public static TheoryData<GameConfiguration> ConfigsWithEveryExtension => new(
    [
        .. Presets.All,                                         // exact win length, pro opening
        new GameConfiguration("Wide rectangle", 19, 9, 6),      // rectangle, longest win
    ]);

    [Fact]
    public void Constructor_CreatesTheFolder()
    {
        Assert.True(Directory.Exists(_folder));
        Assert.Empty(_repository.List());
    }

    [Theory]
    [MemberData(nameof(ConfigsWithEveryExtension))]
    public void SaveThenGet_ReturnsEqualConfiguration(GameConfiguration config)
    {
        _repository.Save(config);

        // records compare every value, ExactWinLength and ProOpening included
        Assert.Equal(config, _repository.Get(config.Name));
    }

    [Fact]
    public void List_ShowsSavedNames_Alphabetically()
    {
        _repository.Save(Presets.Small with { Name = "charlie" });
        _repository.Save(Presets.Small with { Name = "Alpha" });
        _repository.Save(Presets.Small with { Name = "b-board" });

        Assert.Equal(["Alpha", "b-board", "charlie"], _repository.List());
    }

    [Fact]
    public void Save_SameNameTwice_OverwritesInsteadOfDuplicating()
    {
        _repository.Save(Presets.Small with { Name = "Tiny" });
        _repository.Save(Presets.Small with { Name = "TINY", BoardWidth = 19 });

        Assert.Equal(["TINY"], _repository.List());
        Assert.Equal(19, _repository.Get("tiny").BoardWidth);
    }

    [Fact]
    public void Save_InvalidConfiguration_ThrowsAndWritesNothing()
    {
        Assert.Throws<ArgumentException>(() => _repository.Save(Presets.Small with { Name = "Huge", BoardWidth = 50 }));
        Assert.Empty(_repository.List());
    }

    [Fact]
    public void Delete_RemovesConfiguration()
    {
        _repository.Save(Presets.Small);

        _repository.Delete("Small");

        Assert.Empty(_repository.List());
        Assert.Throws<KeyNotFoundException>(() => _repository.Get("Small"));
    }

    [Fact]
    public void Delete_Missing_DoesNothing()
    {
        _repository.Delete("Nothing here");
        _repository.Delete("../not even a valid name");

        Assert.Empty(_repository.List());
    }

    [Theory]
    [InlineData("Missing")]
    [InlineData("../outside")]
    public void Get_Missing_ThrowsKeyNotFound(string name)
    {
        Assert.Throws<KeyNotFoundException>(() => _repository.Get(name));
    }

    [Theory]
    [InlineData("{ this is not json")]                                                   // broken text
    [InlineData("""{ "Name": "Broken", "BoardWidth": 15 }""")]                           // values missing
    [InlineData("""{ "Name": "Broken", "BoardWidth": 99, "BoardHeight": 9, "WinLength": 4 }""")] // breaks the rules
    [InlineData("""{ "Name": "Other", "BoardWidth": 9, "BoardHeight": 9, "WinLength": 4 }""")]   // name doesn't match the file
    public void Get_DamagedFile_ThrowsJsonException(string content)
    {
        File.WriteAllText(Path.Combine(_folder, "broken.config.json"), content);

        Assert.Throws<JsonException>(() => _repository.Get("Broken"));
    }

    [Fact]
    public void SeedMissing_EmptyRepository_AddsAllPresets()
    {
        Presets.SeedMissing(_repository);

        Assert.Equal(["Free-style", "Pro", "Small", "Standard"], _repository.List());
    }

    [Fact]
    public void SeedMissing_RecreatesDeletedPreset_AndKeepsCustomOnes()
    {
        Presets.SeedMissing(_repository);
        _repository.Save(Presets.Small with { Name = "Mine" });
        _repository.Delete("Pro");

        Presets.SeedMissing(_repository);

        Assert.Equal(["Free-style", "Mine", "Pro", "Small", "Standard"], _repository.List());
    }

    [Fact]
    public void SeedMissing_NeverOverwritesStoredPreset()
    {
        _repository.Save(Presets.Small with { WinLength = 5 });

        Presets.SeedMissing(_repository);

        Assert.Equal(5, _repository.Get("Small").WinLength);
    }
}
