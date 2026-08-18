using System.Text.Json;
using System.Text.Json.Serialization;

namespace GameService.Serialization;

public sealed class PileIdJsonConverter : JsonConverter<PileId>
{
    public override PileId Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => PileIds.Parse(reader.GetString() ?? throw new JsonException("Pile ID cannot be null."));

    public override void Write(Utf8JsonWriter writer, PileId value, JsonSerializerOptions options)
        => writer.WriteStringValue(value.ToString());
}
