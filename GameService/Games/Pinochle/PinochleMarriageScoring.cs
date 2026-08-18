using System.Text.Json.Serialization;

namespace GameService.Games.Pinochle;

[JsonConverter(typeof(CamelCaseJsonStringEnumConverter))]
public enum PinochleMarriageScoring
{
    Standard,
    DoubleMarriageBonus
}
