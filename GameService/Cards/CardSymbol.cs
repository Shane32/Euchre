using System.Text.Json.Serialization;

namespace GameService.Cards;

/// <summary>Specifies the supported card symbol values.</summary>
[JsonConverter(typeof(CardSymbolJsonConverter))]
public enum CardSymbol
{
    /// <summary>Represents the clubs card symbol.</summary>
    Clubs,
    /// <summary>Represents the diamonds card symbol.</summary>
    Diamonds,
    /// <summary>Represents the hearts card symbol.</summary>
    Hearts,
    /// <summary>Represents the spades card symbol.</summary>
    Spades,
    /// <summary>Represents the nine card symbol.</summary>
    Nine,
    /// <summary>Represents the ten card symbol.</summary>
    Ten,
    /// <summary>Represents the jack card symbol.</summary>
    Jack,
    /// <summary>Represents the queen card symbol.</summary>
    Queen,
    /// <summary>Represents the king card symbol.</summary>
    King,
    /// <summary>Represents the ace card symbol.</summary>
    Ace
}
