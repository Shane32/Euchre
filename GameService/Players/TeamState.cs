namespace GameService.Players;

public sealed class TeamState
{
    public int TeamId { get; set; }
    public List<int> SeatIds { get; set; } = [];
}
