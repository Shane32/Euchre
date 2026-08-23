using System.Text.Json.Serialization;

namespace GameService.Games.Pinochle;

/// <summary>Specifies the supported pinochle marriage scoring values.</summary>
[JsonConverter(typeof(CamelCaseJsonStringEnumConverter))]
public enum PinochleMarriageScoring
{
    /// <summary>Represents the standard option.</summary>
    Standard,
    /// <summary>Represents the double marriage bonus option.</summary>
    DoubleMarriageBonus
}
