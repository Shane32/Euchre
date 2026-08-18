namespace GameService.Definitions;

public sealed class PlayerConfigurationDefinition
{
    public int MinimumPlayers { get; init; }
    public int MaximumPlayers { get; init; }
    public List<int> Seats { get; init; } = [];
    public List<TeamDefinition> Teams { get; init; } = [];
    public List<int> TurnOrder { get; init; } = [];
    public bool AllowSpectators { get; init; }
    public bool AllowJoinInProgress { get; init; }
}
