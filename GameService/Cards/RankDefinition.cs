namespace GameService.Cards;

public sealed class RankDefinition
{
    public CardRank Id { get; init; }
    public CardSymbol Symbol { get; init; }
    public int BaseOrder { get; init; }
}
