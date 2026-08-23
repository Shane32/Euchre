namespace GameService.Definitions;

/// <summary>Defines phase configuration.</summary>
public sealed class PhaseDefinition
{
    /// <summary>Gets or sets the identifier.</summary>
    public GamePhase Id { get; init; }
    /// <summary>Gets or sets the mode.</summary>
    public PhaseMode Mode { get; init; }
    /// <summary>Gets or sets the actors.</summary>
    public ActorSelector Actors { get; init; }
    /// <summary>Gets or sets the allowed actions.</summary>
    public List<GameActionType> AllowedActions { get; init; } = [];
    /// <summary>Gets or sets the entry handler.</summary>
    public string? EntryHandler { get; init; }
    /// <summary>Gets or sets the completion handler.</summary>
    public string? CompletionHandler { get; init; }
    /// <summary>Gets or sets the timeout policy.</summary>
    public string? TimeoutPolicy { get; init; }
}
