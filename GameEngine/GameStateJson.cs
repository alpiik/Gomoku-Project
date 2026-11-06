using System.Text.Json;
using System.Text.Json.Serialization;

namespace GameEngine;

/// <summary>
/// The one place that turns a game into JSON text and back. The JSON files (DAL.Json) and the database's
/// StateJson column (DAL.EF) both use it, so a game file and a database row contain exactly the same text.
/// No file or database code here: this only converts between objects and text.
/// </summary>
public static class GameStateJson
{
    /// <summary>
    /// Goes up when the layout changes; <see cref="FromJson"/> still reads the older versions.
    /// 1 (A3, A4): no player types. 2 (A5): PlayerX and PlayerO. A version 1 text needs no conversion:
    /// the missing players keep their default, Human, which is exactly what those games were.
    /// </summary>
    public const int CurrentVersion = 2;

    /// <summary>
    /// One shared options object for every read and write (configurations too): the same settings both ways,
    /// and the serializer caches what it learns about each type in it.
    /// </summary>
    public static readonly JsonSerializerOptions Options = new()
    {
        // people can open the files and read them
        WriteIndented = true,

        // "X" instead of 1, so reordering the enum can't break old saves; a number is refused when reading
        Converters = { new JsonStringEnumConverter(allowIntegerValues: false) },

        // a null where C# says "never null" (e.g. "Board": null) is an error now, not a crash later
        RespectNullableAnnotations = true,

        // a configuration without e.g. "BoardWidth" is an error, not a silent 0
        RespectRequiredConstructorParameters = true,
    };

    public static string ToJson(GameState state) =>
        JsonSerializer.Serialize(new VersionedState { Version = CurrentVersion, State = state }, Options);

    /// <summary>
    /// Reads a saved game. <paramref name="source"/> names the file or row in error messages.
    /// Throws <see cref="JsonException"/> for broken or unplayable text and
    /// <see cref="NotSupportedException"/> for text written by a newer program.
    /// </summary>
    public static GameState FromJson(string json, string source)
    {
        VersionedState? saved;
        try
        {
            saved = JsonSerializer.Deserialize<VersionedState>(json, Options);
        }
        catch (JsonException e)
        {
            throw new JsonException($"{source} could not be read: {e.Message}", e);
        }

        if (saved is null)
        {
            throw new JsonException($"{source} is empty.");
        }

        if (saved.Version is < 1 or > CurrentVersion)
        {
            throw new NotSupportedException(
                $"{source} has format version {saved.Version}; this program reads versions 1 to {CurrentVersion}.");
        }

        // hand-edited text must still be playable
        var errors = saved.State.Validate();
        return errors.Count == 0
            ? saved.State
            : throw new JsonException($"{source} is damaged: {string.Join(" ", errors)}");
    }

    /// <summary>What the text contains: the format version next to the state.</summary>
    private sealed class VersionedState
    {
        public int Version { get; init; }
        public required GameState State { get; init; }
    }
}
