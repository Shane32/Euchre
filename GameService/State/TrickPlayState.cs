namespace GameService.State;

/// <summary>Represents the current trick play state.</summary>
public sealed class TrickPlayState
{
    /// <summary>Gets or sets the seat.</summary>
    public int Seat { get; set; }
    /// <summary>Gets or sets the card instance identifier.</summary>
    public required string CardInstanceId { get; set; }
    /// <summary>Gets or sets the order.</summary>
    public int Order { get; set; }
}
