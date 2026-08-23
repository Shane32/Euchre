namespace GameService.Cards;

/// <summary>Defines rank configuration.</summary>
public sealed class RankDefinition
{
    /// <summary>Gets or sets the identifier.</summary>
    public CardRank Id { get; init; }
    /// <summary>Gets or sets the symbol.</summary>
    public CardSymbol Symbol { get; init; }
    /// <summary>Gets or sets the base order.</summary>
    public int BaseOrder { get; init; }
}
