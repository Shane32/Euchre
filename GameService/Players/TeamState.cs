namespace GameService.Players;

/// <summary>Represents the current team state.</summary>
public sealed class TeamState
{
    /// <summary>Gets or sets the team identifier.</summary>
    public int TeamId { get; set; }
    /// <summary>Gets or sets the seat identifiers.</summary>
    public List<int> SeatIds { get; set; } = [];
}
