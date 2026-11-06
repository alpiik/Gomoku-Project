using System.Text.Json;
using System.Text.Json.Serialization;

namespace DAL.Json;

/// <summary>
/// One shared options object for every read and write: the same settings both ways, and the
/// serializer caches what it learns about each type in it.
/// </summary>
public static class JsonDefaults
{
    public static readonly JsonSerializerOptions Options = new()
    {
        // people can open the files and read them
        WriteIndented = true,

        // "X" instead of 1, so reordering the enum can't break old files; a number is refused when reading
        Converters = { new JsonStringEnumConverter(allowIntegerValues: false) },

        // a null where C# says "never null" (e.g. "Board": null) is an error now, not a crash later
        RespectNullableAnnotations = true,

        // a configuration without e.g. "BoardWidth" is an error, not a silent 0
        RespectRequiredConstructorParameters = true,
    };
}
