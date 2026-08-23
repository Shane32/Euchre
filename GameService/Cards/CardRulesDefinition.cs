namespace GameService.Cards;

/// <summary>Defines card rules configuration.</summary>
public sealed class CardRulesDefinition
{
    /// <summary>Gets or sets the effective suit handler.</summary>
    public string? EffectiveSuitHandler { get; init; }
    /// <summary>Gets or sets the follow suit handler.</summary>
    public string? FollowSuitHandler { get; init; }
    /// <summary>Gets or sets the trick winner handler.</summary>
    public string? TrickWinnerHandler { get; init; }
    /// <summary>Gets or sets the trump order.</summary>
    public List<CardOrderRank> TrumpOrder { get; init; } = [];
    /// <summary>Gets or sets the plain suit order.</summary>
    public List<CardOrderRank> PlainSuitOrder { get; init; } = [];
}
