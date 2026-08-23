using System.Text.Json.Serialization;

namespace GameService.Actions;

/// <summary>Specifies the supported action transport values.</summary>
[JsonConverter(typeof(CamelCaseJsonStringEnumConverter))]
public enum ActionTransport
{
    /// <summary>Represents the play card option.</summary>
    PlayCard,
    /// <summary>Represents the other option.</summary>
    Other,
    /// <summary>Represents the system option.</summary>
    System
}
