namespace GameService.State;

public sealed class TricksState
{
    public PileId CurrentPileId { get; set; }
    public int? LeaderSeat { get; set; }
    public CardSuit? LedSuit { get; set; }
    public List<TrickPlayState> Plays { get; set; } = [];
    public List<CompletedTrickState> Completed { get; set; } = [];
    public Dictionary<int, int> WonBySeat { get; set; } = [];
    public Dictionary<int, int> WonByTeam { get; set; } = [];
}
