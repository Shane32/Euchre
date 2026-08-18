using System.Text.Json.Serialization;

namespace GameService.Actions;

[JsonConverter(typeof(CamelCaseJsonStringEnumConverter))]
public enum GameActionType
{
    Ready,
    Deal,
    Pass,
    OrderUp,
    ClaimAceNoFace,
    ChooseTrump,
    ChooseNoTrump,
    Discard,
    PlayCard,
    ScoreDeal,
    Bid
}
