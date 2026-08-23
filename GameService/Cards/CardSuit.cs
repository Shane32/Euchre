using System.Text.Json.Serialization;

namespace GameService.Cards;

/// <summary>Specifies the supported card suit values.</summary>
[JsonConverter(typeof(CamelCaseJsonStringEnumConverter))]
public enum CardSuit
{
    /// <summary>Represents the clubs suit.</summary>
    Clubs,
    /// <summary>Represents the diamonds suit.</summary>
    Diamonds,
    /// <summary>Represents the hearts suit.</summary>
    Hearts,
    /// <summary>Represents the spades suit.</summary>
    Spades
}
