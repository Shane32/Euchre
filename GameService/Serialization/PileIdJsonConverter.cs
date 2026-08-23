using System.Text.Json;
using System.Text.Json.Serialization;

namespace GameService.Serialization;

/// <summary>Converts pile identifier values to and from JSON.</summary>
public sealed class PileIdJsonConverter : JsonConverter<PileId>
{
    /// <summary>Reads and converts a value from JSON.</summary>
    public override PileId Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => PileIds.Parse(reader.GetString() ?? throw new JsonException("Pile ID cannot be null."));

    /// <summary>Writes a value as JSON.</summary>
    public override void Write(Utf8JsonWriter writer, PileId value, JsonSerializerOptions options)
        => writer.WriteStringValue(value.ToString());
}
