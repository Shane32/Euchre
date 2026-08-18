using System.Text.Json.Serialization;

namespace GameService.Piles;

[JsonConverter(typeof(CamelCaseJsonStringEnumConverter))]
public enum PileKind
{
    Stock,
    Waste,
    Hand,
    Kitty,
    Widow,
    Trick,
    Capture,
    Tableau,
    Foundation,
    Meld,
    PassSelection,
    Discard,
    Reserve
}
