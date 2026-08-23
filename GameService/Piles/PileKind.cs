using System.Text.Json.Serialization;

namespace GameService.Piles;

/// <summary>Specifies the supported pile kind values.</summary>
[JsonConverter(typeof(CamelCaseJsonStringEnumConverter))]
public enum PileKind
{
    /// <summary>Represents the stock pile kind.</summary>
    Stock,
    /// <summary>Represents the waste pile kind.</summary>
    Waste,
    /// <summary>Represents the hand pile kind.</summary>
    Hand,
    /// <summary>Represents the kitty pile kind.</summary>
    Kitty,
    /// <summary>Represents the widow pile kind.</summary>
    Widow,
    /// <summary>Represents the trick pile kind.</summary>
    Trick,
    /// <summary>Represents the capture pile kind.</summary>
    Capture,
    /// <summary>Represents the tableau pile kind.</summary>
    Tableau,
    /// <summary>Represents the foundation pile kind.</summary>
    Foundation,
    /// <summary>Represents the meld pile kind.</summary>
    Meld,
    /// <summary>Represents the pass selection pile kind.</summary>
    PassSelection,
    /// <summary>Represents the discard pile kind.</summary>
    Discard,
    /// <summary>Represents the reserve pile kind.</summary>
    Reserve
}
