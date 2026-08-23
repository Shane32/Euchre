namespace GameService.Definitions;

/// <summary>Defines player configuration.</summary>
public sealed class PlayerConfigurationDefinition
{
    /// <summary>Gets or sets the minimum players.</summary>
    public int MinimumPlayers { get; init; }
    /// <summary>Gets or sets the maximum players.</summary>
    public int MaximumPlayers { get; init; }
    /// <summary>Gets or sets the seats.</summary>
    public List<int> Seats { get; init; } = [];
    /// <summary>Gets or sets the teams.</summary>
    public List<TeamDefinition> Teams { get; init; } = [];
    /// <summary>Gets or sets the turn order.</summary>
    public List<int> TurnOrder { get; init; } = [];
    /// <summary>Gets or sets the allow spectators.</summary>
    public bool AllowSpectators { get; init; }
    /// <summary>Gets or sets the allow join in progress.</summary>
    public bool AllowJoinInProgress { get; init; }
}
