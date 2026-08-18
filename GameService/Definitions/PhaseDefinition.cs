namespace GameService.Definitions;

public sealed class PhaseDefinition
{
    public GamePhase Id { get; init; }
    public PhaseMode Mode { get; init; }
    public ActorSelector Actors { get; init; }
    public List<GameActionType> AllowedActions { get; init; } = [];
    public string? EntryHandler { get; init; }
    public string? CompletionHandler { get; init; }
    public string? TimeoutPolicy { get; init; }
}
