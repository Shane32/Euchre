using System.Text.Json.Serialization;

namespace GameService.Players;

/// <summary>Specifies the supported connection status values.</summary>
[JsonConverter(typeof(CamelCaseJsonStringEnumConverter))]
public enum ConnectionStatus
{
    /// <summary>Represents the connected option.</summary>
    Connected,
    /// <summary>Represents the disconnected option.</summary>
    Disconnected
}
