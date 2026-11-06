using DAL.EF;
using GameEngine;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Tests.Persistence;

/// <summary>What only the database has: the foreign key between saved games and configurations, and migrations.</summary>
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
    public void UpgradingAnA4Database_GivesItsGamesHumanPlayers()
    {
        using var connection = new SqliteConnection("Data Source=:memory:");
        connection.Open();
        using var db = new AppDbContext(new DbContextOptionsBuilder<AppDbContext>().UseSqlite(connection).Options);
        var migrator = db.GetService<IMigrator>();

        // the database as A4 left it: only the first migration, a game row without player columns
        migrator.Migrate("InitialCreate");
        var state = GameState.New(Presets.Small, "Saved by A4");
        var id = state.Id.ToString().ToUpperInvariant(); // the way EF writes a Guid into a TEXT column
        var json = OldSaves.Version1Json(state);
        db.Database.ExecuteSql(
            $"INSERT INTO Configurations (Name, BoardWidth, BoardHeight, WinLength, ExactWinLength, ProOpening) VALUES ('Small', 9, 9, 4, 0, 0)");
        db.Database.ExecuteSql(
            $"INSERT INTO SavedGames (Id, Name, SavedAtUtc, StateJson, ConfigurationId) VALUES ({id}, 'Saved by A4', '2026-11-01 12:00:00', {json}, 1)");

        // what Program.cs does at startup
        migrator.Migrate();

        // the column itself, because EF reads text it can't parse (e.g. "") as the enum's default, Human,
        // so the repository alone would hide a wrong default value in the migration
        var columns = db.Database
            .SqlQuery<string>($"SELECT PlayerX || ',' || PlayerO AS Value FROM SavedGames")
            .ToList();
        Assert.Equal(["Human,Human"], columns);

        var games = new GameRepositoryEf(db);
        var summary = Assert.Single(games.List());
        Assert.Equal((EPlayerType.Human, EPlayerType.Human), (summary.PlayerX, summary.PlayerO));
        Assert.Equal(EPlayerType.Human, games.Get(state.Id).PlayerX);
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
