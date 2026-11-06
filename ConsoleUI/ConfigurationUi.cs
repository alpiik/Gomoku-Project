using GameEngine;
using MenuSystem;

namespace ConsoleUI;

/// <summary>
/// Screens for listing, creating, editing and deleting configurations. The public methods return a
/// menu result, so they can be used directly as menu item actions.
/// Presets can be played but not edited or deleted. A configuration that saved games use can't be deleted or
/// renamed: the database forbids it (Restrict foreign key), and this screen checks first so that both
/// storages behave the same and the user gets a clear message.
/// </summary>
public class ConfigurationUi(IConfigRepository repository, IGameRepository games)
{
    public string ListAll()
    {
        ConsoleInput.StartScreen("Configurations");
        foreach (var name in repository.List())
        {
            Console.WriteLine($"  {Line(repository.Get(name))}");
        }

        Console.WriteLine();
        ConsoleInput.Pause();
        return MenuResult.None;
    }

    public string Create()
    {
        ConsoleInput.StartScreen("Create a configuration");
        var config = AskValues(Presets.FreeStyle, currentName: null);
        if (config is not null)
        {
            repository.Save(config);
            Console.WriteLine($"Saved '{config.Name}'.");
            ConsoleInput.Pause();
        }

        return MenuResult.None;
    }

    public string Edit()
    {
        ConsoleInput.StartScreen("Edit a configuration");
        var name = ChooseCustom();
        if (name is null)
        {
            return MenuResult.None;
        }

        var old = repository.Get(name);
        var updated = AskValues(old, currentName: old.Name);
        if (updated is null)
        {
            return MenuResult.None;
        }

        // renamed (more than upper/lower case): remove the old one, otherwise there would be two copies
        if (!updated.Name.Equals(old.Name, StringComparison.OrdinalIgnoreCase))
        {
            if (RefuseIfUsed(old.Name, "renamed"))
            {
                return MenuResult.None;
            }

            repository.Delete(old.Name);
        }

        repository.Save(updated);
        Console.WriteLine($"Saved '{updated.Name}'.");
        ConsoleInput.Pause();
        return MenuResult.None;
    }

    public string Delete()
    {
        ConsoleInput.StartScreen("Delete a configuration");
        var name = ChooseCustom();
        if (name is not null && !RefuseIfUsed(name, "deleted") && ConsoleInput.AskYesNo($"Delete '{name}'?"))
        {
            repository.Delete(name);
            Console.WriteLine($"Deleted '{name}'.");
            ConsoleInput.Pause();
        }

        return MenuResult.None;
    }

    // true (and a message) when saved games use the configuration
    private bool RefuseIfUsed(string name, string action)
    {
        var count = games.List().Count(g => g.ConfigName.Equals(name, StringComparison.OrdinalIgnoreCase));
        if (count == 0)
        {
            return false;
        }

        Console.WriteLine($"'{name}' can't be {action}: {count} saved game(s) use it. Delete those games first.");
        ConsoleInput.Pause();
        return true;
    }

    /// <summary>Lets the player pick any configuration, presets included. Null = cancelled.</summary>
    public GameConfiguration? Choose(string title)
    {
        ConsoleInput.StartScreen(title);
        var names = repository.List();
        var index = ConsoleInput.AskChoice(names.Select(n => Line(repository.Get(n))).ToList());
        return index is null ? null : repository.Get(names[index.Value]);
    }

    private static string Line(GameConfiguration config)
    {
        var preset = Presets.IsPreset(config.Name) ? " (preset)" : "";
        return $"{config.Name,-16} {BoardView.Describe(config)}{preset}";
    }

    // only custom configurations can be edited or deleted
    private string? ChooseCustom()
    {
        var names = repository.List().Where(n => !Presets.IsPreset(n)).ToList();
        if (names.Count == 0)
        {
            Console.WriteLine("There are no custom configurations yet. Presets cannot be edited or deleted.");
            ConsoleInput.Pause();
            return null;
        }

        var index = ConsoleInput.AskChoice(names.Select(n => Line(repository.Get(n))).ToList());
        return index is null ? null : names[index.Value];
    }

    /// <summary>Asks every value, offering the start values as defaults. Null = cancelled.</summary>
    private GameConfiguration? AskValues(GameConfiguration start, string? currentName)
    {
        var name = AskName(currentName);
        if (name is null)
        {
            return null;
        }

        var width = ConsoleInput.AskInt("Board width (columns)",
            GameConfiguration.MinBoardSize, GameConfiguration.MaxBoardSize, start.BoardWidth);
        var height = ConsoleInput.AskInt("Board height (rows)",
            GameConfiguration.MinBoardSize, GameConfiguration.MaxBoardSize, start.BoardHeight);
        var winLength = ConsoleInput.AskInt("Pieces in a row to win",
            GameConfiguration.MinWinLength, GameConfiguration.MaxWinLength, start.WinLength);
        var exact = ConsoleInput.AskYesNo($"Only exactly {winLength} in a row wins, longer lines don't",
            start.ExactWinLength);
        var proOpening = ConsoleInput.AskYesNo(
            $"Pro opening (first move in the centre, X's second move {GameConfiguration.ProOpeningDistance}+ cells away)",
            start.ProOpening);

        var config = new GameConfiguration(name, width, height, winLength)
        {
            ExactWinLength = exact,
            ProOpening = proOpening
        };

        // the questions above already keep the values in range; the engine still has the final say
        var errors = config.Validate();
        if (errors.Count == 0)
        {
            return config;
        }

        Console.WriteLine("Not saved:");
        errors.ForEach(e => Console.WriteLine($"  {e}"));
        ConsoleInput.Pause();
        return null;
    }

    // a new configuration: Enter cancels. An existing one: Enter keeps the current name.
    private string? AskName(string? currentName)
    {
        while (true)
        {
            var name = ConsoleInput.AskText(currentName is null ? "Name (Enter to cancel)" : "Name", currentName);
            if (string.IsNullOrEmpty(name))
            {
                return null;
            }

            var error = GetNameError(name, currentName);
            if (error is null)
            {
                return name;
            }

            Console.WriteLine(error);
        }
    }

    private string? GetNameError(string name, string? currentName)
    {
        if (!GameConfiguration.IsValidName(name))
        {
            return $"Use only letters, digits, spaces, '-' and '_', at most {GameConfiguration.MaxNameLength} characters.";
        }

        var isRenameOfItself = currentName is not null && name.Equals(currentName, StringComparison.OrdinalIgnoreCase);
        var taken = repository.List().Any(n => n.Equals(name, StringComparison.OrdinalIgnoreCase));
        return taken && !isRenameOfItself ? $"A configuration called '{name}' already exists." : null;
    }
}
