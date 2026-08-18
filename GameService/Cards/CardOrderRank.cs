using System.Text.Json.Serialization;

namespace GameService.Cards;

[JsonConverter(typeof(CamelCaseJsonStringEnumConverter))]
public enum CardOrderRank
{
    RightBower,
    LeftBower,
    Ace,
    Ten,
    King,
    Queen,
    Jack,
    Nine
}
