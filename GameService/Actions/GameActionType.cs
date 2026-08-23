using System.Text.Json.Serialization;

namespace GameService.Actions;

/// <summary>Specifies the supported game action type values.</summary>
[JsonConverter(typeof(CamelCaseJsonStringEnumConverter))]
public enum GameActionType
{
    /// <summary>Represents the ready game action.</summary>
    Ready,
    /// <summary>Represents the deal game action.</summary>
    Deal,
    /// <summary>Represents the pass game action.</summary>
    Pass,
    /// <summary>Represents the order up game action.</summary>
    OrderUp,
    /// <summary>Represents the claim ace no face game action.</summary>
    ClaimAceNoFace,
    /// <summary>Represents the choose trump game action.</summary>
    ChooseTrump,
    /// <summary>Represents the choose no trump game action.</summary>
    ChooseNoTrump,
    /// <summary>Represents the discard game action.</summary>
    Discard,
    /// <summary>Represents the play card game action.</summary>
    PlayCard,
    /// <summary>Represents the score deal game action.</summary>
    ScoreDeal,
    /// <summary>Represents the bid game action.</summary>
    Bid
}
