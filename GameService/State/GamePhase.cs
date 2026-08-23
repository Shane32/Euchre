using System.Text.Json.Serialization;

namespace GameService.State;

/// <summary>Specifies the supported game phase values.</summary>
[JsonConverter(typeof(CamelCaseJsonStringEnumConverter))]
public enum GamePhase
{
    /// <summary>Represents the waiting game phase.</summary>
    Waiting,
    /// <summary>Represents the deal game phase.</summary>
    Deal,
    /// <summary>Represents the bid round one game phase.</summary>
    BidRoundOne,
    /// <summary>Represents the dealer discard game phase.</summary>
    DealerDiscard,
    /// <summary>Represents the bid round two game phase.</summary>
    BidRoundTwo,
    /// <summary>Represents the bidding game phase.</summary>
    Bidding,
    /// <summary>Represents the choose trump game phase.</summary>
    ChooseTrump,
    /// <summary>Represents the play game phase.</summary>
    Play,
    /// <summary>Represents the score deal game phase.</summary>
    ScoreDeal,
    /// <summary>Represents the completed game phase.</summary>
    Completed
}
