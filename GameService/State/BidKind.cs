using System.Text.Json.Serialization;

namespace GameService.State;

[JsonConverter(typeof(CamelCaseJsonStringEnumConverter))]
public enum BidKind
{
    Pass,
    Bid,
    OrderUp,
    ChooseTrump,
    ChooseNoTrump
}
