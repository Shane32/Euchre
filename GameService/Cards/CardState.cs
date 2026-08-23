using System.Text.Json;

namespace GameService.Cards;

/// <summary>Represents the current card state.</summary>
public sealed class CardState
{
    /// <summary>Gets or sets the instance identifier.</summary>
    public string? InstanceId { get; set; }
    /// <summary>Gets or sets the ref.</summary>
    public string? Ref { get; set; }
    /// <summary>Gets or sets the card identifier.</summary>
    public string? CardId { get; set; }
    /// <summary>Gets or sets the copy.</summary>
    public int? Copy { get; set; }
    /// <summary>Gets or sets the rank.</summary>
    public CardRank? Rank { get; set; }
    /// <summary>Gets or sets the suit.</summary>
    public CardSuit? Suit { get; set; }
    /// <summary>Gets or sets the face up.</summary>
    public bool FaceUp { get; set; }
    /// <summary>Gets or sets the unknown.</summary>
    public bool Unknown { get; set; }
    /// <summary>Gets or sets the orientation.</summary>
    public CardOrientation? Orientation { get; set; }
    /// <summary>Gets or sets the annotations.</summary>
    public Dictionary<string, JsonElement> Annotations { get; set; } = new(StringComparer.Ordinal);
}
