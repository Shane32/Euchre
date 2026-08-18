namespace GameService.State;

public sealed class CompletedTrickState
{
    public int Number { get; set; }
    public int LeaderSeat { get; set; }
    public int WinnerSeat { get; set; }
    public required string WinningCardInstanceId { get; set; }
    public List<TrickPlayState> Plays { get; set; } = [];
}
