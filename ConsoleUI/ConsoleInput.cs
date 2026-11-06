namespace ConsoleUI;

/// <summary>Small input helpers: ask, check, ask again until the answer is usable.</summary>
public static class ConsoleInput
{
    public static void StartScreen(string title)
    {
        if (!Console.IsOutputRedirected)
        {
            Console.Clear();
        }

        Console.WriteLine(title);
        Console.WriteLine(new string('=', title.Length));
    }

    public static void Pause()
    {
        Console.Write("Press Enter to continue...");
        Console.ReadLine();
    }

    /// <summary>A whole number in min..max. With a current value, Enter keeps it.</summary>
    public static int AskInt(string prompt, int min, int max, int? current = null)
    {
        while (true)
        {
            Console.Write(current is null
                ? $"{prompt} ({min}-{max}): "
                : $"{prompt} ({min}-{max}, Enter keeps {current}): ");
            var input = Console.ReadLine();
            if (input is null)
            {
                return current ?? min; // end of input
            }

            if (input.Trim() == "" && current is not null)
            {
                return current.Value;
            }

            if (int.TryParse(input, out var value) && value >= min && value <= max)
            {
                return value;
            }

            Console.WriteLine($"Please type a number from {min} to {max}.");
        }
    }

    /// <summary>y or n. With a current value, Enter keeps it.</summary>
    public static bool AskYesNo(string prompt, bool? current = null)
    {
        while (true)
        {
            Console.Write(current is null
                ? $"{prompt} (y/n): "
                : $"{prompt} (y/n, Enter keeps {(current.Value ? "y" : "n")}): ");
            var input = Console.ReadLine()?.Trim().ToLowerInvariant();
            switch (input)
            {
                case null:
                    return current ?? false; // end of input
                case "" when current is not null:
                    return current.Value;
                case "y" or "yes":
                    return true;
                case "n" or "no":
                    return false;
            }

            Console.WriteLine("Please type y or n.");
        }
    }

    /// <summary>Trimmed text, or null when the input has ended. With a current value, Enter keeps it.</summary>
    public static string? AskText(string prompt, string? current = null)
    {
        Console.Write(current is null ? $"{prompt}: " : $"{prompt} (Enter keeps \"{current}\"): ");
        var input = Console.ReadLine()?.Trim();
        if (input is null)
        {
            return null;
        }

        return input == "" && current is not null ? current : input;
    }

    /// <summary>Shows a numbered list and returns the chosen index, or null when the user just presses Enter.</summary>
    public static int? AskChoice(IReadOnlyList<string> options)
    {
        for (var i = 0; i < options.Count; i++)
        {
            Console.WriteLine($"{i + 1,3}) {options[i]}");
        }

        while (true)
        {
            Console.Write($"Choose 1-{options.Count} (Enter to cancel): ");
            var input = Console.ReadLine()?.Trim();
            if (string.IsNullOrEmpty(input))
            {
                return null;
            }

            if (int.TryParse(input, out var number) && number >= 1 && number <= options.Count)
            {
                return number - 1;
            }

            Console.WriteLine($"Please type a number from 1 to {options.Count}.");
        }
    }
}
