namespace GameService.State;

public sealed class ScoreState
{
    public Dictionary<string, ScoreEntryState> Players { get; set; } = new(StringComparer.Ordinal);
    public Dictionary<int, ScoreEntryState> Teams { get; set; } = [];
    public int? Target { get; set; }
}
