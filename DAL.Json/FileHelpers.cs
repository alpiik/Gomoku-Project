namespace DAL.Json;

public static class FileHelpers
{
    /// <summary>
    /// Writes to a temporary file first and then renames it. A rename either happens completely or
    /// not at all, so a crash in the middle never leaves half a JSON file behind.
    /// </summary>
    public static void WriteAtomic(string path, string content)
    {
        var temp = path + ".tmp";
        File.WriteAllText(temp, content);
        File.Move(temp, path, overwrite: true);
    }
}
