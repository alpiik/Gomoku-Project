namespace DAL.Json;

/// <summary>
/// Every game file carries a version number. When the layout changes (players arrive in A5),
/// <see cref="Current"/> goes up and <see cref="Check"/> learns to upgrade the older layout.
/// </summary>
public static class SaveFormat
{
    public const int Current = 1;

    /// <summary>Refuses files this program cannot understand instead of loading something wrong.</summary>
    public static void Check(SavedGameFile file, string fileName)
    {
        if (file.Version > Current)
        {
            throw new NotSupportedException(
                $"{fileName} was saved by a newer version of the program (format {file.Version}, this one reads up to {Current}).");
        }

        if (file.Version < 1)
        {
            throw new NotSupportedException($"{fileName} has no valid format version.");
        }
    }
}
