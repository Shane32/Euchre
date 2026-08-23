namespace GameService.Definitions;

/// <summary>Defines team configuration.</summary>
public sealed class TeamDefinition
{
    /// <summary>Gets or sets the team identifier.</summary>
    public int TeamId { get; init; }
    /// <summary>Gets or sets the seat identifiers.</summary>
    public List<int> SeatIds { get; init; } = [];
}
