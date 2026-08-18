namespace GameService.State;

public sealed class TrickPlayState
{
    public int Seat { get; set; }
    public required string CardInstanceId { get; set; }
    public int Order { get; set; }
}
