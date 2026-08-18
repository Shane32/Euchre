namespace GameService.Definitions;

public sealed class TeamDefinition
{
    public int TeamId { get; init; }
    public List<int> SeatIds { get; init; } = [];
}
