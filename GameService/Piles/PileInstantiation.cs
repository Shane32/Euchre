using System.Text.Json.Serialization;

namespace GameService.Piles;

[JsonConverter(typeof(CamelCaseJsonStringEnumConverter))]
public enum PileInstantiation
{
    Once,
    PerSeat,
    PerTeam
}
