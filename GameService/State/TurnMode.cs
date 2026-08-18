using System.Text.Json.Serialization;

namespace GameService.State;

[JsonConverter(typeof(CamelCaseJsonStringEnumConverter))]
public enum TurnMode
{
    Automatic,
    ExclusiveTurn,
    Simultaneous,
    Realtime
}
