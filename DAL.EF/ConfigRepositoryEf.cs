using GameEngine;
using Microsoft.EntityFrameworkCore;

namespace DAL.EF;

/// <summary>Configurations in the database. Same behaviour as ConfigRepositoryJson; the contract tests check that.</summary>
public class ConfigRepositoryEf(AppDbContext db) : IConfigRepository
{
    // sorted in C# with the same comparer as the JSON version: SQLite's NOCASE puts '_' before letters,
    // .NET's ignore-case comparer puts it after them, and both versions must list in exactly the same order
    public List<string> List() =>
        db.Configurations
            .AsNoTracking()
            .Select(c => c.Name)
            .AsEnumerable()
            .Order(StringComparer.OrdinalIgnoreCase)
            .ToList();

    public GameConfiguration Get(string name)
    {
        var row = db.Configurations.AsNoTracking().FirstOrDefault(c => c.Name == name)
                  ?? throw new KeyNotFoundException($"Configuration '{name}' not found.");

        return new GameConfiguration(row.Name, row.BoardWidth, row.BoardHeight, row.WinLength)
        {
            ExactWinLength = row.ExactWinLength,
            ProOpening = row.ProOpening,
        };
    }

    public void Save(GameConfiguration config)
    {
        var errors = config.Validate();
        if (errors.Count > 0)
        {
            throw new ArgumentException(string.Join(" ", errors), nameof(config));
        }

        // upsert: update the row with this name (ignoring case), or insert a new one
        var row = db.Configurations.FirstOrDefault(c => c.Name == config.Name);
        if (row is null)
        {
            row = new Configuration();
            db.Configurations.Add(row);
        }

        row.Name = config.Name;
        row.BoardWidth = config.BoardWidth;
        row.BoardHeight = config.BoardHeight;
        row.WinLength = config.WinLength;
        row.ExactWinLength = config.ExactWinLength;
        row.ProOpening = config.ProOpening;
        db.SaveChangesAndForget();
    }

    public void Delete(string name)
    {
        var row = db.Configurations.FirstOrDefault(c => c.Name == name);
        if (row is null)
        {
            return;
        }

        db.Configurations.Remove(row);
        db.SaveChangesAndForget();
    }
}
