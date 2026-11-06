using System.Text.Json;
using GameEngine;

namespace DAL.Json;

/// <summary>
/// One file per configuration: configs/{name in lower case}.config.json. Lower case makes "Small" and
/// "small" the same file on every operating system (Windows ignores case in file names, Linux doesn't).
/// </summary>
public class ConfigRepositoryJson : IConfigRepository
{
    private const string Suffix = ".config.json";
    private readonly string _folder;

    /// <param name="folder">Tests pass a temporary folder; the program uses the default one.</param>
    public ConfigRepositoryJson(string? folder = null)
    {
        _folder = folder ?? AppPaths.Configs;
        Directory.CreateDirectory(_folder); // does nothing if it already exists
    }

    // the file name is in lower case, so the name as the user typed it is read from inside the file
    public List<string> List() =>
        Directory.EnumerateFiles(_folder, "*" + Suffix)
            .Select(path => Read(path).Name)
            .Order(StringComparer.OrdinalIgnoreCase)
            .ToList();

    public GameConfiguration Get(string name)
    {
        var path = PathFor(name);
        if (path is null || !File.Exists(path))
        {
            throw new KeyNotFoundException($"Configuration '{name}' not found.");
        }

        return Read(path);
    }

    public void Save(GameConfiguration config)
    {
        // Validate also makes sure the name is a safe file name
        var errors = config.Validate();
        if (errors.Count > 0)
        {
            throw new ArgumentException(string.Join(" ", errors), nameof(config));
        }

        FileHelpers.WriteAtomic(PathFor(config.Name)!, JsonSerializer.Serialize(config, JsonDefaults.Options));
    }

    public void Delete(string name)
    {
        var path = PathFor(name);
        if (path is not null)
        {
            File.Delete(path); // no error when the file doesn't exist
        }
    }

    // null for a name no configuration can have, e.g. "../x": such a name must never become a path
    private string? PathFor(string name) =>
        GameConfiguration.IsValidName(name)
            ? Path.Combine(_folder, name.ToLowerInvariant() + Suffix)
            : null;

    private GameConfiguration Read(string path)
    {
        var fileName = Path.GetFileName(path);
        GameConfiguration? config;
        try
        {
            config = JsonSerializer.Deserialize<GameConfiguration>(File.ReadAllText(path), JsonDefaults.Options);
        }
        catch (JsonException e)
        {
            throw new JsonException($"{fileName} could not be read: {e.Message}", e);
        }

        if (config is null)
        {
            throw new JsonException($"{fileName} is empty.");
        }

        // a hand-edited file must still follow the rules, and its name must match the file name
        var errors = config.Validate();
        if (errors.Count == 0 && PathFor(config.Name) != path)
        {
            errors.Add($"The name inside the file is '{config.Name}'.");
        }

        return errors.Count == 0
            ? config
            : throw new JsonException($"{fileName} is damaged: {string.Join(" ", errors)}");
    }
}
