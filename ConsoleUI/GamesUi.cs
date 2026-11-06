using GameEngine;
using MenuSystem;

namespace ConsoleUI;

/// <summary>
/// Screens for starting, continuing, listing and deleting games. Talks only to the interfaces, so it
/// doesn't know (or care) whether the games are in JSON files or a database.
/// </summary>
public class GamesUi(ConfigurationUi configUi, IGameRepository games, MoveProviderFactory players)
{
    // AI vs AI moves instantly; this pause makes the game watchable and gives the stop key time to be pressed
    private static readonly TimeSpan AiVsAiPause = TimeSpan.FromMilliseconds(500);

    private static readonly EPlayerType[] PlayerTypes = Enum.GetValues<EPlayerType>();

    public string NewGame()
    {
        var config = configUi.Choose("New game - choose a configuration");
        if (config is null)
        {
            return MenuResult.None;
        }

        var playerX = AskPlayerType(EGamePiece.X);
        var playerO = playerX is null ? null : AskPlayerType(EGamePiece.O);
        if (playerO is null)
        {
            return MenuResult.None;
        }

        var name = AskGameName($"{config.Name} {DateTime.Now:yyyy-MM-dd HH:mm}");
        return name is null
            ? MenuResult.None
            : Play(GameState.New(config, name, playerX!.Value, playerO.Value));
    }

    /// <summary>A loaded game continues with the players it was saved with, AI included.</summary>
    public string Continue()
    {
        var game = Choose("Load a saved game");
        return game is null ? MenuResult.None : Play(games.Get(game.Id));
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

    // the saved player types decide who plays; the game loop itself never looks at them
    private string Play(GameState state)
    {
        var aiVsAi = state.PlayerX != EPlayerType.Human && state.PlayerO != EPlayerType.Human;
        var run = GameRunner.RunAsync(state, players.Create(state.PlayerX), players.Create(state.PlayerO), games,
            aiVsAi ? AiVsAiPause : TimeSpan.Zero);

        // the menu library is synchronous, so the game is waited for here. A console app has no
        // SynchronizationContext, so this blocking wait can't deadlock.
        return run.GetAwaiter().GetResult();
    }

    // null = cancelled
    private static EPlayerType? AskPlayerType(EGamePiece piece)
    {
        Console.WriteLine();
        Console.WriteLine($"Player {piece}:");
        var index = ConsoleInput.AskChoice(PlayerTypes.Select(BoardView.PlayerName).ToList());
        return index is null ? null : PlayerTypes[index.Value];
    }

    // name, configuration, players, local time of the last save
    private static string Line(GameSummary game) =>
        $"{game.Name,-30} {game.ConfigName,-12} " +
        $"{BoardView.PlayerName(game.PlayerX),-9} vs {BoardView.PlayerName(game.PlayerO),-9} " +
        $"saved {game.SavedAtUtc.ToLocalTime():yyyy-MM-dd HH:mm}";

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
