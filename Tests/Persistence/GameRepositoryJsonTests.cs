using System.Text.Json;
using DAL.Json;
using GameEngine;

namespace Tests.Persistence;

/// <summary>Every test gets its own empty temporary folder, deleted again afterwards (xUnit calls Dispose).</summary>
public class GameRepositoryJsonTests : IDisposable
{
    private readonly string _folder = Path.Combine(Path.GetTempPath(), "gomoku-tests", Guid.NewGuid().ToString());
    private readonly GameRepositoryJson _repository;

    public GameRepositoryJsonTests()
    {
        _repository = new GameRepositoryJson(_folder);
    }

    public void Dispose()
    {
        Directory.Delete(_folder, recursive: true);
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

    private string FileOf(Guid id) => Path.Combine(_folder, id + ".game.json");

    [Fact]
    public void SaveThenGet_ReturnsEqualState()
    {
        var state = PlayedGame(Presets.Pro, "Round trip", new(9, 9), new(9, 10), new(12, 12));

        _repository.Save(state);
        var loaded = _repository.Get(state.Id);

        Assert.Equal(state.Id, loaded.Id);
        Assert.Equal(state.Name, loaded.Name);
        Assert.Equal(state.Config, loaded.Config);     // record: every value, ProOpening included
        Assert.Equal(state.Board, loaded.Board);       // xUnit compares the rows cell by cell
        Assert.Equal(state.NextMoveBy, loaded.NextMoveBy);
        Assert.Equal(state.Moves, loaded.Moves);
        Assert.Equal(state.CreatedAtUtc, loaded.CreatedAtUtc);
    }

    public static TheoryData<GameConfiguration> ConfigsWithEveryExtension => new(
    [
        .. Presets.All,
        new GameConfiguration("Wide rectangle", 19, 9, 6),
    ]);

    [Theory]
    [MemberData(nameof(ConfigsWithEveryExtension))]
    public void SaveThenGet_KeepsTheConfiguration(GameConfiguration config)
    {
        var state = GameState.New(config);

        _repository.Save(state);

        Assert.Equal(config, _repository.Get(state.Id).Config);
    }

    [Fact]
    public void LoadedGame_ContinuesExactlyLikeTheOriginal()
    {
        // Small: 4 in a row wins. X has three in the top row, X to move.
        var original = PlayedGame(Presets.Small, "Almost won",
            new(0, 0), new(1, 0), new(0, 1), new(1, 1), new(0, 2), new(1, 2));
        _repository.Save(original);

        var originalBrain = new GameBrain(original);
        var loadedBrain = new GameBrain(_repository.Get(original.Id));

        Assert.Equal(originalBrain.GetLegalMoves(), loadedBrain.GetLegalMoves());
        Assert.Equal(EGameStatus.XWon, originalBrain.MakeMove(0, 3));
        Assert.Equal(EGameStatus.XWon, loadedBrain.MakeMove(0, 3));
    }

    [Fact]
    public void LoadedProGame_StillEnforcesTheOpeningRule()
    {
        var state = PlayedGame(Presets.Pro, "Pro opening", new(9, 9), new(9, 10));
        _repository.Save(state);

        var loadedBrain = new GameBrain(_repository.Get(state.Id));

        Assert.False(loadedBrain.IsLegalMove(10, 10)); // X's second move too close to the centre
        Assert.True(loadedBrain.IsLegalMove(12, 12));
    }

    [Fact]
    public void List_ShowsNameConfigurationAndTime_NewestFirst()
    {
        var before = DateTime.UtcNow;
        var first = PlayedGame(Presets.Small, "First", new Move(4, 4));
        var second = PlayedGame(Presets.Standard, "Second", new Move(7, 7));
        _repository.Save(first);
        _repository.Save(second);
        var after = DateTime.UtcNow;

        var list = _repository.List();

        Assert.Equal([second.Id, first.Id], list.Select(g => g.Id));
        Assert.Equal("Second", list[0].Name);
        Assert.Equal("Standard", list[0].ConfigName);
        Assert.InRange(list[0].SavedAtUtc, before, after);
    }

    [Fact]
    public void Save_SameGameTwice_KeepsOneFileWithTheLatestState()
    {
        var state = PlayedGame(Presets.Small, "Autosaved", new Move(4, 4));
        _repository.Save(state);
        new GameBrain(state).MakeMove(4, 5);

        _repository.Save(state);

        Assert.Single(_repository.List());
        Assert.Single(Directory.GetFiles(_folder)); // no .tmp file left behind either
        Assert.Equal(2, _repository.Get(state.Id).Moves.Count);
    }

    [Fact]
    public void Delete_RemovesGame()
    {
        var state = GameState.New(Presets.Small);
        _repository.Save(state);

        _repository.Delete(state.Id);

        Assert.Empty(_repository.List());
        Assert.Throws<KeyNotFoundException>(() => _repository.Get(state.Id));
    }

    [Fact]
    public void Delete_Missing_DoesNothing()
    {
        _repository.Delete(Guid.NewGuid());

        Assert.Empty(_repository.List());
    }

    [Fact]
    public void Get_Missing_ThrowsKeyNotFound()
    {
        Assert.Throws<KeyNotFoundException>(() => _repository.Get(Guid.NewGuid()));
    }

    [Fact]
    public void SavedFile_StoresEnumsAsText_AndAFormatVersion()
    {
        var state = PlayedGame(Presets.Small, "Readable", new Move(4, 4));

        _repository.Save(state);
        var json = File.ReadAllText(FileOf(state.Id));

        Assert.Contains("\"NextMoveBy\": \"O\"", json);
        Assert.Contains("\"X\"", json);
        Assert.Contains($"\"Version\": {SaveFormat.Current}", json);
    }

    [Fact]
    public void Get_FileFromNewerProgram_ThrowsNotSupported()
    {
        var state = GameState.New(Presets.Small);
        _repository.Save(state);
        var path = FileOf(state.Id);
        File.WriteAllText(path, File.ReadAllText(path).Replace($"\"Version\": {SaveFormat.Current}", "\"Version\": 99"));

        Assert.Throws<NotSupportedException>(() => _repository.Get(state.Id));
    }

    [Fact]
    public void Get_BrokenText_ThrowsJsonException()
    {
        var id = Guid.NewGuid();
        File.WriteAllText(FileOf(id), "{ \"Version\": 1, \"State\": ");

        Assert.Throws<JsonException>(() => _repository.Get(id));
    }

    [Fact]
    public void Get_UnplayableState_ThrowsJsonException()
    {
        var state = GameState.New(Presets.Small);
        state.Board = GameState.CreateBoard(3, 3); // doesn't match the 9x9 configuration
        _repository.Save(state);

        Assert.Throws<JsonException>(() => _repository.Get(state.Id));
    }
}
