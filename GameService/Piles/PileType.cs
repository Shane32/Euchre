using System.Text.Json.Serialization;

namespace GameService.Piles;

/// <summary>Specifies the supported pile type values.</summary>
[JsonConverter(typeof(CamelCaseJsonStringEnumConverter))]
public enum PileType
{
    /// <summary>Represents the deck pile type.</summary>
    Deck,
    /// <summary>Represents the stock pile type.</summary>
    Stock,
    /// <summary>Represents the waste pile type.</summary>
    Waste,
    /// <summary>Represents the hand pile type.</summary>
    Hand,
    /// <summary>Represents the kitty pile type.</summary>
    Kitty,
    /// <summary>Represents the widow pile type.</summary>
    Widow,
    /// <summary>Represents the current trick pile type.</summary>
    CurrentTrick,
    /// <summary>Represents the won pile type.</summary>
    Won,
    /// <summary>Represents the capture pile type.</summary>
    Capture,
    /// <summary>Represents the tableau pile type.</summary>
    Tableau,
    /// <summary>Represents the foundation pile type.</summary>
    Foundation,
    /// <summary>Represents the meld pile type.</summary>
    Meld,
    /// <summary>Represents the pass selection pile type.</summary>
    PassSelection,
    /// <summary>Represents the discard pile type.</summary>
    Discard,
    /// <summary>Represents the reserve pile type.</summary>
    Reserve
}
