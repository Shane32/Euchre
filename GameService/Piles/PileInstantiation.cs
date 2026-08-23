using System.Text.Json.Serialization;

namespace GameService.Piles;

/// <summary>Specifies the supported pile instantiation values.</summary>
[JsonConverter(typeof(CamelCaseJsonStringEnumConverter))]
public enum PileInstantiation
{
    /// <summary>Represents the once option.</summary>
    Once,
    /// <summary>Represents the per seat option.</summary>
    PerSeat,
    /// <summary>Represents the per team option.</summary>
    PerTeam
}
