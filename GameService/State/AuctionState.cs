namespace GameService.State;

/// <summary>Represents the current auction state.</summary>
public sealed class AuctionState
{
    /// <summary>Gets or sets the round.</summary>
    public int Round { get; set; }
    /// <summary>Gets or sets the active seat.</summary>
    public int? ActiveSeat { get; set; }
    /// <summary>Gets or sets the calls.</summary>
    public List<BidState> Calls { get; set; } = [];
    /// <summary>Gets or sets the high call.</summary>
    public BidState? HighCall { get; set; }
    /// <summary>Gets or sets the passed seats.</summary>
    public List<int> PassedSeats { get; set; } = [];
    /// <summary>Gets or sets the complete.</summary>
    public bool Complete { get; set; }
}
