using System.Text.Json;
using DAL.Json;
using GameEngine;

namespace Tests.Persistence;

/// <summary>What only the JSON files have: folders, file names and files people can edit by hand.</summary>
public class JsonStorageTests : IDisposable
{
    private readonly TestStorage _storage = new();

    public void Dispose() => _storage.Dispose();

    private string ConfigFolder => Path.Combine(_storage.Folder, "configs");
    private string GameFolder => Path.Combine(_storage.Folder, "games");

    [Fact]
    public void Repositories_CreateTheirFolders()
    {
        _storage.Configs(TestStorage.Json);
        _storage.Games(TestStorage.Json);

        Assert.True(Directory.Exists(ConfigFolder));
        Assert.True(Directory.Exists(GameFolder));
    }

    [Fact]
    public void ConfigFile_IsNamedAfterTheConfigurationInLowerCase()
    {
        _storage.Configs(TestStorage.Json).Save(Presets.FreeStyle);

        Assert.True(File.Exists(Path.Combine(ConfigFolder, "free-style.config.json")));
    }

    [Theory]
    [InlineData("{ this is not json")]                                                            // broken text
    [InlineData("""{ "Name": "Broken", "BoardWidth": 15 }""")]                                    // values missing
    [InlineData("""{ "Name": "Broken", "BoardWidth": 99, "BoardHeight": 9, "WinLength": 4 }""")] // breaks the rules
    [InlineData("""{ "Name": "Other", "BoardWidth": 9, "BoardHeight": 9, "WinLength": 4 }""")]   // name doesn't match the file
    public void DamagedConfigFile_ThrowsJsonException(string content)
    {
        var repository = _storage.Configs(TestStorage.Json);
        File.WriteAllText(Path.Combine(ConfigFolder, "broken.config.json"), content);

        Assert.Throws<JsonException>(() => repository.Get("Broken"));
    }

    [Fact]
    public void DamagedGameFile_ThrowsJsonException()
    {
        var repository = _storage.Games(TestStorage.Json);
        var id = Guid.NewGuid();
        File.WriteAllText(Path.Combine(GameFolder, id + ".game.json"), "{ \"Version\": 1, \"State\": ");

        Assert.Throws<JsonException>(() => repository.Get(id));
    }

    [Fact]
    public void GameFile_WithAnotherGamesId_ThrowsJsonException()
    {
        var repository = _storage.Games(TestStorage.Json);
        var state = GameState.New(Presets.Small);
        repository.Save(state);
        var copy = Guid.NewGuid();
        File.Copy(Path.Combine(GameFolder, state.Id + ".game.json"), Path.Combine(GameFolder, copy + ".game.json"));

        Assert.Throws<JsonException>(() => repository.Get(copy));
    }

    [Fact]
    public void Save_SameGameTwice_LeavesOneFileAndNoTemporaryFile()
    {
        var repository = _storage.Games(TestStorage.Json);
        var state = GameState.New(Presets.Small);

        repository.Save(state);
        repository.Save(state);

        Assert.Equal([Path.Combine(GameFolder, state.Id + ".game.json")], Directory.GetFiles(GameFolder));
    }

    [Fact]
    public void GameFile_ContainsExactlyTheSharedJson()
    {
        var repository = new GameRepositoryJson(GameFolder);
        var state = GameState.New(Presets.Small);

        repository.Save(state);

        Assert.Equal(GameStateJson.ToJson(state), File.ReadAllText(Path.Combine(GameFolder, state.Id + ".game.json")));
    }
}
