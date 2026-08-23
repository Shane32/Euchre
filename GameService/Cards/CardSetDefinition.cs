namespace GameService.Cards;

/// <summary>Defines card set configuration.</summary>
public sealed class CardSetDefinition
{
    /// <summary>Gets or sets the copies.</summary>
    public int Copies { get; init; } = 1;
    /// <summary>Gets or sets the suits.</summary>
    public List<SuitDefinition> Suits { get; init; } = [];
    /// <summary>Gets or sets the ranks.</summary>
    public List<RankDefinition> Ranks { get; init; } = [];
    /// <summary>Gets or sets the construction.</summary>
    public required CardConstructionDefinition Construction { get; init; }
}
