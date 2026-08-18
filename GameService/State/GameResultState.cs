namespace GameService.State;

public sealed class GameResultState
{
    public List<string> WinnerPlayerIds { get; set; } = [];
    public List<int> WinnerTeamIds { get; set; } = [];
    public GameResultReason Reason { get; set; }
    public DateTimeOffset CompletedAt { get; set; }
}
