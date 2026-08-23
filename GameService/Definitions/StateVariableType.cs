using System.Text.Json.Serialization;

namespace GameService.Definitions;

/// <summary>Specifies the supported state variable type values.</summary>
[JsonConverter(typeof(CamelCaseJsonStringEnumConverter))]
public enum StateVariableType
{
    /// <summary>Represents the boolean option.</summary>
    Boolean,
    /// <summary>Represents the integer option.</summary>
    Integer,
    /// <summary>Represents the string option.</summary>
    String,
    /// <summary>Represents the timestamp option.</summary>
    Timestamp,
    /// <summary>Represents the seat identifier option.</summary>
    SeatId,
    /// <summary>Represents the team identifier option.</summary>
    TeamId,
    /// <summary>Represents the pile identifier option.</summary>
    PileId,
    /// <summary>Represents the card instance identifier option.</summary>
    CardInstanceId,
    /// <summary>Represents the suit identifier option.</summary>
    SuitId,
    /// <summary>Represents the rank identifier option.</summary>
    RankId,
    /// <summary>Represents the array option.</summary>
    Array,
    /// <summary>Represents the map option.</summary>
    Map,
    /// <summary>Represents the object option.</summary>
    Object,
    /// <summary>Represents the JSON option.</summary>
    Json
}
