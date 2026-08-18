using System.Text.Json.Serialization;

namespace GameService.State;

[JsonConverter(typeof(CamelCaseJsonStringEnumConverter))]
public enum GamePhase
{
    Waiting,
    Deal,
    BidRoundOne,
    DealerDiscard,
    BidRoundTwo,
    Bidding,
    ChooseTrump,
    Play,
    ScoreDeal,
    Completed
}
