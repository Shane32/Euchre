namespace GameService.State;

/// <summary>Represents the current score state.</summary>
public sealed class ScoreState
{
    /// <summary>Gets or sets the players.</summary>
    public Dictionary<string, ScoreEntryState> Players { get; set; } = new(StringComparer.Ordinal);
    /// <summary>Gets or sets the teams.</summary>
    public Dictionary<int, ScoreEntryState> Teams { get; set; } = [];
    /// <summary>Gets or sets the target.</summary>
    public int? Target { get; set; }
}
