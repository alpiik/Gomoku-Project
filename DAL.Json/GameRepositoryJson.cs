using System.Text.Json;
using GameEngine;

namespace DAL.Json;

/// <summary>One file per game: games/{id}.game.json. A Guid is always a valid file name.</summary>
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
            .OrderByDescending(file => file.SavedAtUtc)
            .Select(file => new GameSummary(file.State.Id, file.State.Name, file.State.Config.Name, file.SavedAtUtc))
            .ToList();

    public GameState Get(Guid id)
    {
        var path = PathFor(id);
        if (!File.Exists(path))
        {
            throw new KeyNotFoundException($"Saved game {id} not found.");
        }

        return Read(path).State;
    }

    public void Save(GameState state)
    {
        var file = new SavedGameFile
        {
            Version = SaveFormat.Current,
            SavedAtUtc = DateTime.UtcNow,
            State = state,
        };
        FileHelpers.WriteAtomic(PathFor(state.Id), JsonSerializer.Serialize(file, JsonDefaults.Options));
    }

    public void Delete(Guid id) => File.Delete(PathFor(id)); // no error when the file doesn't exist

    private string PathFor(Guid id) => Path.Combine(_folder, id + Suffix);

    private SavedGameFile Read(string path)
    {
        var fileName = Path.GetFileName(path);
        SavedGameFile? file;
        try
        {
            file = JsonSerializer.Deserialize<SavedGameFile>(File.ReadAllText(path), JsonDefaults.Options);
        }
        catch (JsonException e)
        {
            throw new JsonException($"{fileName} could not be read: {e.Message}", e);
        }

        if (file is null)
        {
            throw new JsonException($"{fileName} is empty.");
        }

        SaveFormat.Check(file, fileName);

        // a hand-edited file must still be playable, and its id must match the file name
        var errors = file.State.Validate();
        if (PathFor(file.State.Id) != path)
        {
            errors.Add($"The id inside the file is {file.State.Id}.");
        }

        return errors.Count == 0
            ? file
            : throw new JsonException($"{fileName} is damaged: {string.Join(" ", errors)}");
    }
}
