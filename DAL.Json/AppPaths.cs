namespace DAL.Json;

/// <summary>
/// Where the files go: the user's home folder, never the working directory or bin/.
/// Windows: C:\Users\name\.icd0008\gomoku, macOS and Linux: ~/.icd0008/gomoku.
/// </summary>
public static class AppPaths
{
    public static string Root { get; } = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
        ".icd0008", "gomoku");

    public static string Configs => Path.Combine(Root, "configs");
    public static string Games => Path.Combine(Root, "games");
}
