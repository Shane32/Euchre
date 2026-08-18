using System.Text.Json.Serialization;

namespace GameService.Definitions;

[JsonConverter(typeof(CamelCaseJsonStringEnumConverter))]
public enum PhaseMode
{
    Automatic,
    ExclusiveTurn,
    Simultaneous
}
