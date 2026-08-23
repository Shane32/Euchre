namespace GameService.State;

/// <summary>Represents the current turn state.</summary>
public sealed class TurnState
{
    /// <summary>Gets or sets the mode.</summary>
    public TurnMode Mode { get; set; } = TurnMode.Automatic;
    /// <summary>Gets or sets the active seats.</summary>
    public List<int> ActiveSeats { get; set; } = [];
    /// <summary>Gets or sets the priority seat.</summary>
    public int? PrioritySeat { get; set; }
    /// <summary>Gets or sets the direction.</summary>
    public TurnDirection Direction { get; set; } = TurnDirection.Clockwise;
    /// <summary>Gets or sets the lead seat.</summary>
    public int? LeadSeat { get; set; }
    /// <summary>Gets or sets the deadline.</summary>
    public DateTimeOffset? Deadline { get; set; }
}
