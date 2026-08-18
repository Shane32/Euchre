namespace GameService.Definitions;

public sealed class ScoringDefinition
{
    public ScoreScope? Scope { get; init; }
    public string? TargetOption { get; init; }
    public List<ScoringOutcomeDefinition> Outcomes { get; init; } = [];
    public string? Handler { get; init; }
}
