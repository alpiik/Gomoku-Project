using GameEngine;

namespace Tests;

/// <summary>Text as the A3/A4 program wrote it, for checking that A5 still loads old saves.</summary>
public static class OldSaves
{
    /// <summary>
    /// Format version 1: today's text without the PlayerX and PlayerO lines (the only thing version 2 added).
    /// </summary>
    public static string Version1Json(GameState state)
    {
        var lines = GameStateJson.ToJson(state)
            .Split('\n')
            .Where(line => !line.Contains("\"PlayerX\"") && !line.Contains("\"PlayerO\""));
        var json = string.Join('\n', lines)
            .Replace($"\"Version\": {GameStateJson.CurrentVersion}", "\"Version\": 1");

        Assert.DoesNotContain("Player", json);
        Assert.Contains("\"Version\": 1", json);
        return json;
    }
}
