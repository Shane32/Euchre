using System.Text.Json.Serialization;

namespace GameService.State;

/// <summary>Specifies the supported trump kind values.</summary>
[JsonConverter(typeof(CamelCaseJsonStringEnumConverter))]
public enum TrumpKind
{
    /// <summary>Represents the suit option.</summary>
    Suit,
    /// <summary>Represents the no trump option.</summary>
    NoTrump
}
