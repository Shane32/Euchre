namespace GameService.State;

public sealed class ScoreEntryState
{
    public int Deal { get; set; }
    public int Total { get; set; }
    public Dictionary<ScoreCounter, int> Counters { get; set; } = [];
}
