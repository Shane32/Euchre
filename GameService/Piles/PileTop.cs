using System.Text.Json.Serialization;

namespace GameService.Piles;

/// <summary>Specifies the supported pile top values.</summary>
[JsonConverter(typeof(CamelCaseJsonStringEnumConverter))]
public enum PileTop
{
    /// <summary>Represents the first option.</summary>
    First,
    /// <summary>Represents the last option.</summary>
    Last
}
