namespace GameService.Cards;

/// <summary>Defines suit configuration.</summary>
public sealed class SuitDefinition
{
    /// <summary>Gets or sets the identifier.</summary>
    public CardSuit Id { get; init; }
    /// <summary>Gets or sets the symbol.</summary>
    public CardSymbol Symbol { get; init; }
    /// <summary>Gets or sets the color.</summary>
    public CardColor? Color { get; init; }
}
