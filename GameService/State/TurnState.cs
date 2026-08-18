namespace GameService.State;

public sealed class TurnState
{
    public TurnMode Mode { get; set; } = TurnMode.Automatic;
    public List<int> ActiveSeats { get; set; } = [];
    public int? PrioritySeat { get; set; }
    public TurnDirection Direction { get; set; } = TurnDirection.Clockwise;
    public int? LeadSeat { get; set; }
    public DateTimeOffset? Deadline { get; set; }
}
