using System.Text.Json.Serialization;

namespace GameService.Cards;

[JsonConverter(typeof(CamelCaseJsonStringEnumConverter))]
public enum CardSuit
{
    Clubs,
    Diamonds,
    Hearts,
    Spades
}
