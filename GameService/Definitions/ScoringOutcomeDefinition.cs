namespace GameService.Definitions;

/// <summary>Defines scoring outcome configuration.</summary>
public sealed class ScoringOutcomeDefinition
{
    /// <summary>Gets or sets the condition.</summary>
    public required string Condition { get; init; }
    /// <summary>Gets or sets the maker points.</summary>
    public int MakerPoints { get; init; }
    /// <summary>Gets or sets the defender points.</summary>
    public int DefenderPoints { get; init; }
}
