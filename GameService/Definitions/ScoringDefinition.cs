namespace GameService.Definitions;

/// <summary>Defines scoring configuration.</summary>
public sealed class ScoringDefinition
{
    /// <summary>Gets or sets the scope.</summary>
    public ScoreScope? Scope { get; init; }
    /// <summary>Gets or sets the target option.</summary>
    public string? TargetOption { get; init; }
    /// <summary>Gets or sets the outcomes.</summary>
    public List<ScoringOutcomeDefinition> Outcomes { get; init; } = [];
    /// <summary>Gets or sets the handler.</summary>
    public string? Handler { get; init; }
}
