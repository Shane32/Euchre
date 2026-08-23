namespace GameService.State;

/// <summary>Represents the current bid state.</summary>
public sealed class BidState
{
    /// <summary>Gets or sets the seat.</summary>
    public int Seat { get; set; }
    /// <summary>Gets or sets the kind.</summary>
    public BidKind Kind { get; set; }
    /// <summary>Gets or sets the value.</summary>
    public int? Value { get; set; }
    /// <summary>Gets or sets the suit.</summary>
    public CardSuit? Suit { get; set; }
    /// <summary>Gets or sets the alone.</summary>
    public bool Alone { get; set; }
    /// <summary>Gets or sets the revision.</summary>
    public long Revision { get; set; }
}
