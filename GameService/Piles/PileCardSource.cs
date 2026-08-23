using System.Text.Json.Serialization;

namespace GameService.Piles;

/// <summary>Specifies the supported pile card source values.</summary>
[JsonConverter(typeof(CamelCaseJsonStringEnumConverter))]
public enum PileCardSource
{
    /// <summary>Represents the all option.</summary>
    All
}
