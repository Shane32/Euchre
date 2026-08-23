namespace GameService.Piles;

/// <summary>Represents the current pile owner state.</summary>
public sealed class PileOwnerState
{
    /// <summary>Gets or sets the kind.</summary>
    public PileOwnerKind Kind { get; set; }
    /// <summary>Gets or sets the seat.</summary>
    public int? Seat { get; set; }
    /// <summary>Gets or sets the team.</summary>
    public int? Team { get; set; }
}
