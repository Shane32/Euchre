using System.Text.Json.Serialization;

namespace GameService.State;

[JsonConverter(typeof(CamelCaseJsonStringEnumConverter))]
public enum ExchangeDirection
{
    Left,
    Right,
    Across,
    Hold,
    Clockwise,
    Counterclockwise
}
