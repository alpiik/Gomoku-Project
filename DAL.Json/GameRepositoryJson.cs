using System.Text.Json;
using GameEngine;

namespace DAL.Json;

/// <summary>
/// One file per game: games/{id}.game.json. A Guid is always a valid file name.
/// The text inside comes from <see cref="GameStateJson"/>, the same code the database uses.
/// </summary>
public class GameRepositoryJson : IGameRepository
{
    private const string Suffix = ".game.json";
    private readonly string _folder;

    /// <param name="folder">Tests pass a temporary folder; the program uses the default one.</param>
    public GameRepositoryJson(string? folder = null)
    {
        _folder = folder ?? AppPaths.Games;
        Directory.CreateDirectory(_folder); // does nothing if it already exists
    }

    // with files every game has to be opened to read its name and time; a database does this in one query
    public List<GameSummary> List() =>
        Directory.EnumerateFiles(_folder, "*" + Suffix)
            .Select(Read)
            .OrderByDescending(state => state.SavedAtUtc)
            .Select(state => new GameSummary(
                state.Id, state.Name, state.Config.Name, state.PlayerX, state.PlayerO, state.SavedAtUtc))
            .ToList();

    public GameState Get(Guid id)
    {
        var path = PathFor(id);
        if (!File.Exists(path))
        {
            throw new KeyNotFoundException($"Saved game {id} not found.");
        }

        return Read(path);
    }

    public void Save(GameState state)
    {
        state.SavedAtUtc = DateTime.UtcNow;
        FileHelpers.WriteAtomic(PathFor(state.Id), GameStateJson.ToJson(state));
    }

    public void Delete(Guid id) => File.Delete(PathFor(id)); // no error when the file doesn't exist

    private string PathFor(Guid id) => Path.Combine(_folder, id + Suffix);

    private GameState Read(string path)
    {
        var fileName = Path.GetFileName(path);
        var state = GameStateJson.FromJson(File.ReadAllText(path), fileName);

        // the id inside must match the file name, otherwise Get and Save would use different files
        return PathFor(state.Id) == path
            ? state
            : throw new JsonException($"{fileName} is damaged: the id inside the file is {state.Id}.");
    }
}
