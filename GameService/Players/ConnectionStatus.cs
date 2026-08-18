using System.Text.Json.Serialization;

namespace GameService.Players;

[JsonConverter(typeof(CamelCaseJsonStringEnumConverter))]
public enum ConnectionStatus
{
    Connected,
    Disconnected
}
