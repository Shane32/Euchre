using System.Text.Json.Serialization;

namespace GameService.State;

/// <summary>Specifies the supported contract kind values.</summary>
[JsonConverter(typeof(CamelCaseJsonStringEnumConverter))]
public enum ContractKind
{
    /// <summary>Represents the euchre option.</summary>
    Euchre,
    /// <summary>Represents the pinochle option.</summary>
    Pinochle,
    /// <summary>Represents the spades option.</summary>
    Spades,
    /// <summary>Represents the hearts option.</summary>
    Hearts
}
