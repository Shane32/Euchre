using System.Text.Json.Serialization;

namespace GameService.Piles;

/// <summary>Specifies the supported pile owner kind values.</summary>
[JsonConverter(typeof(CamelCaseJsonStringEnumConverter))]
public enum PileOwnerKind
{
    /// <summary>Represents the server option.</summary>
    Server,
    /// <summary>Represents the shared option.</summary>
    Shared,
    /// <summary>Represents the seat option.</summary>
    Seat,
    /// <summary>Represents the team option.</summary>
    Team
}
