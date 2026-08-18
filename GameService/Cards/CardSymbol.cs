using System.Text.Json.Serialization;

namespace GameService.Cards;

[JsonConverter(typeof(CardSymbolJsonConverter))]
public enum CardSymbol
{
    Clubs,
    Diamonds,
    Hearts,
    Spades,
    Nine,
    Ten,
    Jack,
    Queen,
    King,
    Ace
}
