namespace GameService.Cards;

public sealed class SuitDefinition
{
    public CardSuit Id { get; init; }
    public CardSymbol Symbol { get; init; }
    public CardColor? Color { get; init; }
}
