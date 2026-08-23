using System.Text.Json.Serialization;

namespace GameService.State;

/// <summary>Specifies the supported bid kind values.</summary>
[JsonConverter(typeof(CamelCaseJsonStringEnumConverter))]
public enum BidKind
{
    /// <summary>Represents the pass option.</summary>
    Pass,
    /// <summary>Represents the bid option.</summary>
    Bid,
    /// <summary>Represents the order up option.</summary>
    OrderUp,
    /// <summary>Represents the choose trump option.</summary>
    ChooseTrump,
    /// <summary>Represents the choose no trump option.</summary>
    ChooseNoTrump
}
