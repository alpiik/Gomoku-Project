using DAL.EF;
using DAL.Json;
using GameEngine;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Tests.Persistence;

/// <summary>
/// Fresh, empty storage for one test: a temporary folder for the JSON files and an in-memory SQLite database
/// with the real migrations applied. Tests ask for a storage by name ("Json" or "Db"), so one test method
/// runs against both implementations. Dispose throws everything away.
/// </summary>
public sealed class TestStorage : IDisposable
{
    public const string Json = "Json";
    public const string Db = "Db";

    public static TheoryData<string> Kinds => [Json, Db];

    /// <summary>Configurations covering every Gomoku extension: exact win length, pro opening, a rectangle.</summary>
    public static IReadOnlyList<GameConfiguration> EveryExtension { get; } =
        [.. Presets.All, new GameConfiguration("Wide rectangle", 19, 9, 6)];

    public string Folder { get; } = Path.Combine(Path.GetTempPath(), "gomoku-tests", Guid.NewGuid().ToString());

    private SqliteConnection? _connection;
    private AppDbContext? _db;

    public IConfigRepository Configs(string kind) => kind switch
    {
        Json => new ConfigRepositoryJson(Path.Combine(Folder, "configs")),
        Db => new ConfigRepositoryEf(Database()),
        _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, "Use \"Json\" or \"Db\"."),
    };

    public IGameRepository Games(string kind) => kind switch
    {
        Json => new GameRepositoryJson(Path.Combine(Folder, "games")),
        Db => new GameRepositoryEf(Database()),
        _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, "Use \"Json\" or \"Db\"."),
    };

    /// <summary>The in-memory database. It exists as long as its connection stays open.</summary>
    public AppDbContext Database()
    {
        if (_db is null)
        {
            _connection = new SqliteConnection("Data Source=:memory:");
            _connection.Open();
            _db = new AppDbContext(new DbContextOptionsBuilder<AppDbContext>().UseSqlite(_connection).Options);
            _db.Database.Migrate(); // the real migration, so the tests check it too
        }

        return _db;
    }

    public void Dispose()
    {
        _db?.Dispose();
        _connection?.Dispose();
        if (Directory.Exists(Folder))
        {
            Directory.Delete(Folder, recursive: true);
        }
    }
}
