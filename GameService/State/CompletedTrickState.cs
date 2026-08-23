namespace GameService.State;

/// <summary>Represents the current completed trick state.</summary>
public sealed class CompletedTrickState
{
    /// <summary>Gets or sets the number.</summary>
    public int Number { get; set; }
    /// <summary>Gets or sets the leader seat.</summary>
    public int LeaderSeat { get; set; }
    /// <summary>Gets or sets the winner seat.</summary>
    public int WinnerSeat { get; set; }
    /// <summary>Gets or sets the winning card instance identifier.</summary>
    public required string WinningCardInstanceId { get; set; }
    /// <summary>Gets or sets the plays.</summary>
    public List<TrickPlayState> Plays { get; set; } = [];
}
