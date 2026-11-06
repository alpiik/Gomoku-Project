using System.Text.Json;
using GameEngine;

namespace Tests.Persistence;

/// <summary>
/// The IGameRepository contract. Every test runs twice, with "Json" and with "Db": both implementations
/// must pass exactly the same tests. Each test gets its own empty storage (xUnit creates a new instance per test).
/// </summary>
public class GameRepositoryContractTests : IDisposable
{
    private readonly TestStorage _storage = new();

    public void Dispose() => _storage.Dispose();

    public static TheoryData<string> Storages => TestStorage.Kinds;

    public static TheoryData<string, GameConfiguration> StoragesAndConfigs()
    {
        var data = new TheoryData<string, GameConfiguration>();
        foreach (var storage in TestStorage.Kinds)
        {
            foreach (var config in TestStorage.EveryExtension)
            {
                data.Add(storage, config);
            }
        }

        return data;
    }

    // the database needs a game's configuration to be stored (foreign key), as it always is in the program
    private IGameRepository Games(string storage)
    {
        var configs = _storage.Configs(storage);
        foreach (var config in TestStorage.EveryExtension)
        {
            configs.Save(config);
        }

        return _storage.Games(storage);
    }

    private static GameState PlayedGame(GameConfiguration config, string name, params Move[] moves)
    {
        var state = GameState.New(config, name);
        var brain = new GameBrain(state);
        foreach (var move in moves)
        {
            brain.MakeMove(move.Row, move.Col);
        }

        return state;
    }

    [Theory]
    [MemberData(nameof(Storages))]
    public void SaveThenGet_ReturnsEqualState(string storage)
    {
        var repository = Games(storage);
        var state = PlayedGame(Presets.Pro, "Round trip", new(9, 9), new(9, 10), new(12, 12));

        repository.Save(state);
        var loaded = repository.Get(state.Id);

        Assert.Equal(state.Id, loaded.Id);
        Assert.Equal(state.Name, loaded.Name);
        Assert.Equal(state.Config, loaded.Config);     // record: every value, ProOpening included
        Assert.Equal(state.Board, loaded.Board);       // xUnit compares the rows cell by cell
        Assert.Equal(state.NextMoveBy, loaded.NextMoveBy);
        Assert.Equal(state.Moves, loaded.Moves);
        Assert.Equal(state.CreatedAtUtc, loaded.CreatedAtUtc);
        Assert.Equal(state.SavedAtUtc, loaded.SavedAtUtc);
    }

    [Theory]
    [MemberData(nameof(StoragesAndConfigs))]
    public void SaveThenGet_KeepsTheConfiguration(string storage, GameConfiguration config)
    {
        var repository = Games(storage);
        var state = GameState.New(config);

        repository.Save(state);

        Assert.Equal(config, repository.Get(state.Id).Config);
    }

    [Theory]
    [MemberData(nameof(Storages))]
    public void LoadedGame_ContinuesExactlyLikeTheOriginal(string storage)
    {
        var repository = Games(storage);

        // Small: 4 in a row wins. X has three in the top row, X to move.
        var original = PlayedGame(Presets.Small, "Almost won",
            new(0, 0), new(1, 0), new(0, 1), new(1, 1), new(0, 2), new(1, 2));
        repository.Save(original);

        var originalBrain = new GameBrain(original);
        var loadedBrain = new GameBrain(repository.Get(original.Id));

        Assert.Equal(originalBrain.GetLegalMoves(), loadedBrain.GetLegalMoves());
        Assert.Equal(EGameStatus.XWon, originalBrain.MakeMove(0, 3));
        Assert.Equal(EGameStatus.XWon, loadedBrain.MakeMove(0, 3));
    }

    [Theory]
    [MemberData(nameof(Storages))]
    public void LoadedProGame_StillEnforcesTheOpeningRule(string storage)
    {
        var repository = Games(storage);
        var state = PlayedGame(Presets.Pro, "Pro opening", new(9, 9), new(9, 10));
        repository.Save(state);

        var loadedBrain = new GameBrain(repository.Get(state.Id));

        Assert.False(loadedBrain.IsLegalMove(10, 10)); // X's second move too close to the centre
        Assert.True(loadedBrain.IsLegalMove(12, 12));
    }

    [Theory]
    [MemberData(nameof(Storages))]
    public void List_ShowsNameConfigurationAndTime_NewestFirst(string storage)
    {
        var repository = Games(storage);
        var before = DateTime.UtcNow;
        var first = PlayedGame(Presets.Small, "First", new Move(4, 4));
        var second = PlayedGame(Presets.Standard, "Second", new Move(7, 7));
        repository.Save(first);
        repository.Save(second);
        var after = DateTime.UtcNow;

        var list = repository.List();

        Assert.Equal([second.Id, first.Id], list.Select(g => g.Id));
        Assert.Equal("Second", list[0].Name);
        Assert.Equal("Standard", list[0].ConfigName);
        Assert.InRange(list[0].SavedAtUtc, before, after);
        Assert.Equal(DateTimeKind.Utc, list[0].SavedAtUtc.Kind); // so ToLocalTime() shows the right time
    }

    [Theory]
    [MemberData(nameof(Storages))]
    public void Save_SameGameTwice_KeepsOneEntryWithTheLatestState(string storage)
    {
        var repository = Games(storage);
        var state = PlayedGame(Presets.Small, "Autosaved", new Move(4, 4));
        repository.Save(state);
        new GameBrain(state).MakeMove(4, 5);

        repository.Save(state);

        Assert.Single(repository.List());
        Assert.Equal(2, repository.Get(state.Id).Moves.Count);
    }

    [Theory]
    [MemberData(nameof(Storages))]
    public void Delete_RemovesGame(string storage)
    {
        var repository = Games(storage);
        var state = GameState.New(Presets.Small);
        repository.Save(state);

        repository.Delete(state.Id);

        Assert.Empty(repository.List());
        Assert.Throws<KeyNotFoundException>(() => repository.Get(state.Id));
    }

    [Theory]
    [MemberData(nameof(Storages))]
    public void Delete_Missing_DoesNothing(string storage)
    {
        var repository = Games(storage);

        repository.Delete(Guid.NewGuid());

        Assert.Empty(repository.List());
    }

    [Theory]
    [MemberData(nameof(Storages))]
    public void Get_Missing_ThrowsKeyNotFound(string storage)
    {
        var repository = Games(storage);

        Assert.Throws<KeyNotFoundException>(() => repository.Get(Guid.NewGuid()));
    }

    [Theory]
    [MemberData(nameof(Storages))]
    public void Get_UnplayableState_ThrowsJsonException(string storage)
    {
        var repository = Games(storage);
        var state = GameState.New(Presets.Small);
        state.Board = GameState.CreateBoard(3, 3); // doesn't match the 9x9 configuration
        repository.Save(state);

        Assert.Throws<JsonException>(() => repository.Get(state.Id));
    }
}
