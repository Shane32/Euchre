namespace GameService.State;

/// <summary>Represents the current tricks state.</summary>
public sealed class TricksState
{
    /// <summary>Gets or sets the current pile identifier.</summary>
    public PileId CurrentPileId { get; set; }
    /// <summary>Gets or sets the leader seat.</summary>
    public int? LeaderSeat { get; set; }
    /// <summary>Gets or sets the led suit.</summary>
    public CardSuit? LedSuit { get; set; }
    /// <summary>Gets or sets the plays.</summary>
    public List<TrickPlayState> Plays { get; set; } = [];
    /// <summary>Gets or sets the completed.</summary>
    public List<CompletedTrickState> Completed { get; set; } = [];
    /// <summary>Gets or sets the winning seat.</summary>
    public Dictionary<int, int> WonBySeat { get; set; } = [];
    /// <summary>Gets or sets the winning team.</summary>
    public Dictionary<int, int> WonByTeam { get; set; } = [];
}
