using System.Text.Json.Serialization;

namespace GameService.Definitions;

/// <summary>Specifies the supported phase mode values.</summary>
[JsonConverter(typeof(CamelCaseJsonStringEnumConverter))]
public enum PhaseMode
{
    /// <summary>Represents the automatic option.</summary>
    Automatic,
    /// <summary>Represents the exclusive turn option.</summary>
    ExclusiveTurn,
    /// <summary>Represents the simultaneous option.</summary>
    Simultaneous
}
