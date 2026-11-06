using GameEngine;
using MenuSystem;

namespace ConsoleUI;

/// <summary>
/// Screens for starting, continuing, listing and deleting games. Talks only to the interfaces, so it
/// doesn't know (or care) whether the games are in JSON files or a database.
/// </summary>
public class GamesUi(ConfigurationUi configUi, IGameRepository games, IMoveProvider human)
{
    public string NewGame()
    {
        var config = configUi.Choose("New game - choose a configuration");
        if (config is null)
        {
            return MenuResult.None;
        }

        var name = AskGameName($"{config.Name} {DateTime.Now:yyyy-MM-dd HH:mm}");
        return name is null
            ? MenuResult.None
            : GameRunner.Run(GameState.New(config, name), human, human, games);
    }

    public string Continue()
    {
        var game = Choose("Load a saved game");
        return game is null
            ? MenuResult.None
            : GameRunner.Run(games.Get(game.Id), human, human, games);
    }

    public string ListAll()
    {
        ConsoleInput.StartScreen("Saved games");
        var saved = games.List();
        if (saved.Count == 0)
        {
            Console.WriteLine("No saved games yet.");
        }

        foreach (var game in saved)
        {
            Console.WriteLine($"  {Line(game)}");
        }

        Console.WriteLine();
        ConsoleInput.Pause();
        return MenuResult.None;
    }

    public string Delete()
    {
        var game = Choose("Delete a saved game");
        if (game is not null && ConsoleInput.AskYesNo($"Delete '{game.Name}'?"))
        {
            games.Delete(game.Id);
            Console.WriteLine($"Deleted '{game.Name}'.");
            ConsoleInput.Pause();
        }

        return MenuResult.None;
    }

    private GameSummary? Choose(string title)
    {
        ConsoleInput.StartScreen(title);
        var saved = games.List();
        if (saved.Count == 0)
        {
            Console.WriteLine("No saved games yet.");
            ConsoleInput.Pause();
            return null;
        }

        var index = ConsoleInput.AskChoice(saved.Select(Line).ToList());
        return index is null ? null : saved[index.Value];
    }

    // name, configuration, local time of the last save
    private static string Line(GameSummary game) =>
        $"{game.Name,-30} {game.ConfigName,-12} saved {game.SavedAtUtc.ToLocalTime():yyyy-MM-dd HH:mm}";

    // null = cancelled (end of input)
    private static string? AskGameName(string defaultName)
    {
        while (true)
        {
            var name = ConsoleInput.AskText("Game name", defaultName);
            if (name is null || name.Length <= GameState.MaxNameLength)
            {
                return name;
            }

            Console.WriteLine($"The name can be at most {GameState.MaxNameLength} characters.");
        }
    }
}
