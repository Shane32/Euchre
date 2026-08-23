using System.Text.Json.Serialization;

namespace GameService.Piles;

/// <summary>Specifies the supported pile selection values.</summary>
[JsonConverter(typeof(CamelCaseJsonStringEnumConverter))]
public enum PileSelection
{
    /// <summary>Represents the none option.</summary>
    None,
    /// <summary>Represents the top option.</summary>
    Top,
    /// <summary>Represents the bottom option.</summary>
    Bottom,
    /// <summary>Represents the any option.</summary>
    Any,
    /// <summary>Represents the euchre kitty option.</summary>
    EuchreKitty
}
