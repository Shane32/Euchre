using System.Text.Json.Serialization;

namespace GameService.State;

[JsonConverter(typeof(CamelCaseJsonStringEnumConverter))]
public enum TurnDirection
{
    Clockwise,
    Counterclockwise
}
