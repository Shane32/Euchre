using System.Text.Json.Serialization;

namespace GameService.Cards;

/// <summary>Specifies the supported card order rank values.</summary>
[JsonConverter(typeof(CamelCaseJsonStringEnumConverter))]
public enum CardOrderRank
{
    /// <summary>Represents the right bower card ordering rank.</summary>
    RightBower,
    /// <summary>Represents the left bower card ordering rank.</summary>
    LeftBower,
    /// <summary>Represents the ace card ordering rank.</summary>
    Ace,
    /// <summary>Represents the ten card ordering rank.</summary>
    Ten,
    /// <summary>Represents the king card ordering rank.</summary>
    King,
    /// <summary>Represents the queen card ordering rank.</summary>
    Queen,
    /// <summary>Represents the jack card ordering rank.</summary>
    Jack,
    /// <summary>Represents the nine card ordering rank.</summary>
    Nine
}
