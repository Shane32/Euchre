using System.Text.Json.Serialization;

namespace GameService.State;

/// <summary>Specifies the supported turn direction values.</summary>
[JsonConverter(typeof(CamelCaseJsonStringEnumConverter))]
public enum TurnDirection
{
    /// <summary>Represents the clockwise option.</summary>
    Clockwise,
    /// <summary>Represents the counterclockwise option.</summary>
    Counterclockwise
}
