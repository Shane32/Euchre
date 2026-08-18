using System.Text.Json.Serialization;

namespace GameService.Piles;

[JsonConverter(typeof(CamelCaseJsonStringEnumConverter))]
public enum PileType
{
    Deck,
    Stock,
    Waste,
    Hand,
    Kitty,
    Widow,
    CurrentTrick,
    Won,
    Capture,
    Tableau,
    Foundation,
    Meld,
    PassSelection,
    Discard,
    Reserve
}
