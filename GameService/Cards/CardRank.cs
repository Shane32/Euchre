using System.Text.Json.Serialization;

namespace GameService.Cards;

[JsonConverter(typeof(CamelCaseJsonStringEnumConverter))]
public enum CardRank
{
    Nine,
    Ten,
    Jack,
    Queen,
    King,
    Ace
}
