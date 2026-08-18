namespace GameService.Cards;

public sealed class CardSetDefinition
{
    public int Copies { get; init; } = 1;
    public List<SuitDefinition> Suits { get; init; } = [];
    public List<RankDefinition> Ranks { get; init; } = [];
    public required CardConstructionDefinition Construction { get; init; }
}
