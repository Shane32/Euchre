using System.Text.Json.Serialization;

namespace GameService.State;

/// <summary>Specifies the supported turn mode values.</summary>
[JsonConverter(typeof(CamelCaseJsonStringEnumConverter))]
public enum TurnMode
{
    /// <summary>Represents the automatic option.</summary>
    Automatic,
    /// <summary>Represents the exclusive turn option.</summary>
    ExclusiveTurn,
    /// <summary>Represents the simultaneous option.</summary>
    Simultaneous,
    /// <summary>Represents the realtime option.</summary>
    Realtime
}
