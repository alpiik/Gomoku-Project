using System.Text.Json;
using MenuSystem;

namespace ConsoleUI;

/// <summary>
/// Menu actions that read or write files can fail: a damaged or missing file, no permission, a full disk.
/// Guard turns those failures into a message, so one bad file never crashes the whole program.
/// Only these storage errors are caught; a bug in the code still stops the program and shows where it is.
/// </summary>
public static class StorageErrors
{
    public static Func<string> Guard(Func<string> action) => () =>
    {
        try
        {
            return action();
        }
        catch (Exception e) when (e is IOException or UnauthorizedAccessException or JsonException
                                      or NotSupportedException or KeyNotFoundException)
        {
            Console.WriteLine();
            Console.WriteLine($"Storage problem: {e.Message}");
            ConsoleInput.Pause();
            return MenuResult.None;
        }
    };
}
