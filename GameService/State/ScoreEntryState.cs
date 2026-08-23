namespace GameService.State;

/// <summary>Represents the current score entry state.</summary>
public sealed class ScoreEntryState
{
    /// <summary>Gets or sets the deal.</summary>
    public int Deal { get; set; }
    /// <summary>Gets or sets the total.</summary>
    public int Total { get; set; }
    /// <summary>Gets or sets the counters.</summary>
    public Dictionary<ScoreCounter, int> Counters { get; set; } = [];
}
