using System.Text.Json.Serialization;

namespace GameService.Piles;

/// <summary>Specifies the supported shuffle algorithm values.</summary>
[JsonConverter(typeof(CamelCaseJsonStringEnumConverter))]
public enum ShuffleAlgorithm
{
    /// <summary>Represents the seeded option.</summary>
    Seeded
}
