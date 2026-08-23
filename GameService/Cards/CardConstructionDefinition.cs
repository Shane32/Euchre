namespace GameService.Cards;

/// <summary>Defines card construction configuration.</summary>
public sealed class CardConstructionDefinition
{
    /// <summary>Gets or sets the kind.</summary>
    public CardConstructionKind Kind { get; init; }
    /// <summary>Gets or sets the card identifier format.</summary>
    public string CardIdFormat { get; init; } = "{rank}.{suit}";
    /// <summary>Gets or sets the cards.</summary>
    public List<ExplicitCardDefinition>? Cards { get; init; }
}
