using System.Text.Json.Serialization;

namespace GameService.State;

/// <summary>Specifies the supported exchange kind values.</summary>
[JsonConverter(typeof(CamelCaseJsonStringEnumConverter))]
public enum ExchangeKind
{
    /// <summary>Represents the pass option.</summary>
    Pass,
    /// <summary>Represents the discard option.</summary>
    Discard,
    /// <summary>Represents the trade option.</summary>
    Trade
}
