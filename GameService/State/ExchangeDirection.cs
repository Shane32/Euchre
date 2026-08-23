using System.Text.Json.Serialization;

namespace GameService.State;

/// <summary>Specifies the supported exchange direction values.</summary>
[JsonConverter(typeof(CamelCaseJsonStringEnumConverter))]
public enum ExchangeDirection
{
    /// <summary>Represents the left option.</summary>
    Left,
    /// <summary>Represents the right option.</summary>
    Right,
    /// <summary>Represents the across option.</summary>
    Across,
    /// <summary>Represents the hold option.</summary>
    Hold,
    /// <summary>Represents the clockwise option.</summary>
    Clockwise,
    /// <summary>Represents the counterclockwise option.</summary>
    Counterclockwise
}
