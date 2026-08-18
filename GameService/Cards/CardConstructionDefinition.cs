namespace GameService.Cards;

public sealed class CardConstructionDefinition
{
    public CardConstructionKind Kind { get; init; }
    public string CardIdFormat { get; init; } = "{rank}.{suit}";
    public List<ExplicitCardDefinition>? Cards { get; init; }
}
