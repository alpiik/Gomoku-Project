using Microsoft.Data.Sqlite;

namespace DAL.EF;

public static class DbPaths
{
    /// <summary>
    /// ~/.icd0008/gomoku/gomoku.db, next to the JSON folders. Built from the user's home folder on every
    /// machine, never a path from this laptop. Creates the folder: SQLite creates the file, not missing folders.
    /// </summary>
    public static string DefaultConnectionString()
    {
        var folder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            ".icd0008", "gomoku");
        Directory.CreateDirectory(folder);
        return new SqliteConnectionStringBuilder { DataSource = Path.Combine(folder, "gomoku.db") }.ToString();
    }
}
