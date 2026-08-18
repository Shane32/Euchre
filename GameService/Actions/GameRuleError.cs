using System.Text.Json.Serialization;

namespace GameService.Actions;

[JsonConverter(typeof(CamelCaseJsonStringEnumConverter))]
public enum GameRuleError
{
    InvalidPhase,
    InvalidCard,
    NotAuthorized,
    NotYourTurn,
    UnsupportedAction,
    PlayerNotFound,
    PileNotFound,
    ActionIdConflict,
    RevisionConflict,
    InvalidAction,
    InvalidDiscard,
    InvalidPlay,
    MustFollowSuit,
    DealerMustBid,
    InvalidTrump,
    InvalidNoTrump,
    InvalidAceNoFace,
    InvalidArguments,
    AlreadyPassed,
    BidTooLow,
    TrumpMarriageRequired,
    IllegalPlay
}
