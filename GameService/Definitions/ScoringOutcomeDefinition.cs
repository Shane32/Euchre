namespace GameService.Definitions;

public sealed class ScoringOutcomeDefinition
{
    public required string Condition { get; init; }
    public int MakerPoints { get; init; }
    public int DefenderPoints { get; init; }
}
