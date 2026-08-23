using System.Text.Json.Serialization;

namespace GameService.Cards;

/// <summary>Specifies the supported card rank values.</summary>
[JsonConverter(typeof(CamelCaseJsonStringEnumConverter))]
public enum CardRank
{
    /// <summary>Represents the nine rank.</summary>
    Nine,
    /// <summary>Represents the ten rank.</summary>
    Ten,
    /// <summary>Represents the jack rank.</summary>
    Jack,
    /// <summary>Represents the queen rank.</summary>
    Queen,
    /// <summary>Represents the king rank.</summary>
    King,
    /// <summary>Represents the ace rank.</summary>
    Ace
}
