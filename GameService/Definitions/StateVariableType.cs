using System.Text.Json.Serialization;

namespace GameService.Definitions;

[JsonConverter(typeof(CamelCaseJsonStringEnumConverter))]
public enum StateVariableType
{
    Boolean,
    Integer,
    String,
    Timestamp,
    SeatId,
    TeamId,
    PileId,
    CardInstanceId,
    SuitId,
    RankId,
    Array,
    Map,
    Object,
    Json
}
