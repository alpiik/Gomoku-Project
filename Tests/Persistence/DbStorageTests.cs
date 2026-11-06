using GameEngine;
using Microsoft.EntityFrameworkCore;

namespace Tests.Persistence;

/// <summary>What only the database has: the foreign key between saved games and configurations.</summary>
public class DbStorageTests : IDisposable
{
    private readonly TestStorage _storage = new();

    public void Dispose() => _storage.Dispose();

    [Fact]
    public void DeletingAConfigurationThatGamesUse_IsRefused_AndTheNextOperationsStillWork()
    {
        var configs = _storage.Configs(TestStorage.Db);
        var games = _storage.Games(TestStorage.Db);
        configs.Save(Presets.Small);
        var state = GameState.New(Presets.Small);
        games.Save(state);

        // Restrict foreign key: the database refuses, nothing is deleted
        Assert.Throws<DbUpdateException>(() => configs.Delete("Small"));
        Assert.Equal(["Small"], configs.List());

        // the refused delete must not stay queued and break the next save
        games.Save(state);
        Assert.Single(games.List());
    }

    [Fact]
    public void SavingAGameWhoseConfigurationIsNotStored_Throws()
    {
        var games = _storage.Games(TestStorage.Db);

        Assert.Throws<KeyNotFoundException>(() => games.Save(GameState.New(Presets.Small)));
    }

    [Fact]
    public void GameRow_ContainsExactlyTheSharedJson()
    {
        _storage.Configs(TestStorage.Db).Save(Presets.Small);
        var state = GameState.New(Presets.Small);

        _storage.Games(TestStorage.Db).Save(state);

        var row = _storage.Database().SavedGames.AsNoTracking().Single();
        Assert.Equal(GameStateJson.ToJson(state), row.StateJson);
    }

    [Fact]
    public void GameMovedFromJsonToDatabase_KeepsItsId()
    {
        var state = GameState.New(Presets.Small, "Moving");
        _storage.Games(TestStorage.Json).Save(state);
        _storage.Configs(TestStorage.Db).Save(Presets.Small);

        var fromJson = _storage.Games(TestStorage.Json).Get(state.Id);
        _storage.Games(TestStorage.Db).Save(fromJson);

        Assert.Equal("Moving", _storage.Games(TestStorage.Db).Get(state.Id).Name);
    }
}
