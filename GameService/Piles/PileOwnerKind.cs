using System.Text.Json.Serialization;

namespace GameService.Piles;

[JsonConverter(typeof(CamelCaseJsonStringEnumConverter))]
public enum PileOwnerKind
{
    Server,
    Shared,
    Seat,
    Team
}
