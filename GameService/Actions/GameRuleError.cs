using System.Text.Json.Serialization;

namespace GameService.Actions;

/// <summary>Specifies the supported game rule error values.</summary>
[JsonConverter(typeof(CamelCaseJsonStringEnumConverter))]
public enum GameRuleError
{
    /// <summary>Represents the invalid phase option.</summary>
    InvalidPhase,
    /// <summary>Represents the invalid card option.</summary>
    InvalidCard,
    /// <summary>Represents the not authorized option.</summary>
    NotAuthorized,
    /// <summary>Represents the not your turn option.</summary>
    NotYourTurn,
    /// <summary>Represents the unsupported action option.</summary>
    UnsupportedAction,
    /// <summary>Represents the player not found option.</summary>
    PlayerNotFound,
    /// <summary>Represents the pile not found option.</summary>
    PileNotFound,
    /// <summary>Represents the action identifier conflict option.</summary>
    ActionIdConflict,
    /// <summary>Represents the revision conflict option.</summary>
    RevisionConflict,
    /// <summary>Represents the invalid action option.</summary>
    InvalidAction,
    /// <summary>Represents the invalid discard option.</summary>
    InvalidDiscard,
    /// <summary>Represents the invalid play option.</summary>
    InvalidPlay,
    /// <summary>Represents the must follow suit option.</summary>
    MustFollowSuit,
    /// <summary>Represents the dealer must bid option.</summary>
    DealerMustBid,
    /// <summary>Represents the invalid trump option.</summary>
    InvalidTrump,
    /// <summary>Represents the invalid no trump option.</summary>
    InvalidNoTrump,
    /// <summary>Represents the invalid ace no face option.</summary>
    InvalidAceNoFace,
    /// <summary>Represents the invalid arguments option.</summary>
    InvalidArguments,
    /// <summary>Represents the already passed option.</summary>
    AlreadyPassed,
    /// <summary>Represents the bid too low option.</summary>
    BidTooLow,
    /// <summary>Represents the trump marriage required option.</summary>
    TrumpMarriageRequired,
    /// <summary>Represents the illegal play option.</summary>
    IllegalPlay
}
