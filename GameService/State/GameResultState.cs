namespace GameService.State;

/// <summary>Represents the current game result state.</summary>
public sealed class GameResultState
{
    /// <summary>Gets or sets the winner player identifiers.</summary>
    public List<string> WinnerPlayerIds { get; set; } = [];
    /// <summary>Gets or sets the winner team identifiers.</summary>
    public List<int> WinnerTeamIds { get; set; } = [];
    /// <summary>Gets or sets the reason.</summary>
    public GameResultReason Reason { get; set; }
    /// <summary>Gets or sets the completed at.</summary>
    public DateTimeOffset CompletedAt { get; set; }
}
