using GameEngine;

namespace ConsoleUI;

/// <summary>Keeps configurations in memory, starting with the presets. Replaced by JSON files in A3.</summary>
public class ConfigRepositoryInMemory : IConfigRepository
{
    // names are compared ignoring case, so "small" and "Small" can't both exist
    private readonly Dictionary<string, GameConfiguration> _configs = new(StringComparer.OrdinalIgnoreCase);

    public ConfigRepositoryInMemory()
    {
        foreach (var preset in Presets.All)
        {
            Save(preset);
        }
    }

    public List<string> List() => _configs.Keys.Order(StringComparer.OrdinalIgnoreCase).ToList();

    public GameConfiguration Get(string name) =>
        _configs.TryGetValue(name, out var config)
            ? config
            : throw new KeyNotFoundException($"Configuration '{name}' not found.");

    public void Save(GameConfiguration config) => _configs[config.Name] = config;

    public void Delete(string name) => _configs.Remove(name);
}
