using System.Text.Json;
using GameEngine;
using Microsoft.EntityFrameworkCore;

namespace DAL.EF;

/// <summary>Saved games in the database. Same behaviour as GameRepositoryJson; the contract tests check that.</summary>
public class GameRepositoryEf(AppDbContext db) : IGameRepository
{
    // one query: only the list columns plus the configuration name through the foreign key (a JOIN),
    // never the StateJson column
    public List<GameSummary> List() =>
        db.SavedGames
            .AsNoTracking()
            .OrderByDescending(g => g.SavedAtUtc)
            .Select(g => new GameSummary(g.Id, g.Name, g.Configuration!.Name, g.PlayerX, g.PlayerO, g.SavedAtUtc))
            .ToList();

    public GameState Get(Guid id)
    {
        var json = db.SavedGames.AsNoTracking().Where(g => g.Id == id).Select(g => g.StateJson).FirstOrDefault()
                   ?? throw new KeyNotFoundException($"Saved game {id} not found.");

        var source = $"Saved game {id}";
        var state = GameStateJson.FromJson(json, source);
        return state.Id == id
            ? state
            : throw new JsonException($"{source} is damaged: the id inside the JSON is {state.Id}.");
    }

    /// <summary>
    /// Inserts or updates the game. The game's configuration must be stored, because the foreign key needs it;
    /// in the program it always is, since a game can only be started from a stored configuration.
    /// </summary>
    public void Save(GameState state)
    {
        var configId = db.Configurations
                           .Where(c => c.Name == state.Config.Name)
                           .Select(c => (int?)c.Id)
                           .FirstOrDefault()
                       ?? throw new KeyNotFoundException(
                           $"The configuration '{state.Config.Name}' of this game is not stored.");

        state.SavedAtUtc = DateTime.UtcNow;
        var row = db.SavedGames.Find(state.Id);
        if (row is null)
        {
            row = new SavedGame { Id = state.Id };
            db.SavedGames.Add(row);
        }

        row.Name = state.Name;
        row.ConfigurationId = configId;
        row.SavedAtUtc = state.SavedAtUtc;
        row.PlayerX = state.PlayerX;
        row.PlayerO = state.PlayerO;
        row.StateJson = GameStateJson.ToJson(state);
        db.SaveChangesAndForget();
    }

    public void Delete(Guid id)
    {
        var row = db.SavedGames.Find(id);
        if (row is null)
        {
            return;
        }

        db.SavedGames.Remove(row);
        db.SaveChangesAndForget();
    }
}
