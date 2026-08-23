using System.Text.Json;
using System.Text.Json.Serialization;

namespace GameService.Serialization;

/// <summary>Converts card symbol values to and from JSON.</summary>
public sealed class CardSymbolJsonConverter : JsonConverter<CardSymbol>
{
    /// <summary>Reads and converts a value from JSON.</summary>
    public override CardSymbol Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => reader.GetString() switch {
            "C" => CardSymbol.Clubs,
            "D" => CardSymbol.Diamonds,
            "H" => CardSymbol.Hearts,
            "S" => CardSymbol.Spades,
            "9" => CardSymbol.Nine,
            "10" => CardSymbol.Ten,
            "J" => CardSymbol.Jack,
            "Q" => CardSymbol.Queen,
            "K" => CardSymbol.King,
            "A" => CardSymbol.Ace,
            var value => throw new JsonException($"Unknown card symbol '{value}'.")
        };

    /// <summary>Writes a value as JSON.</summary>
    public override void Write(Utf8JsonWriter writer, CardSymbol value, JsonSerializerOptions options)
        => writer.WriteStringValue(value switch {
            CardSymbol.Clubs => "C",
            CardSymbol.Diamonds => "D",
            CardSymbol.Hearts => "H",
            CardSymbol.Spades => "S",
            CardSymbol.Nine => "9",
            CardSymbol.Ten => "10",
            CardSymbol.Jack => "J",
            CardSymbol.Queen => "Q",
            CardSymbol.King => "K",
            CardSymbol.Ace => "A",
            _ => throw new JsonException($"Unknown card symbol '{value}'.")
        });
}
