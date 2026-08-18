namespace GameService.Cards;

public sealed class CardRulesDefinition
{
    public string? EffectiveSuitHandler { get; init; }
    public string? FollowSuitHandler { get; init; }
    public string? TrickWinnerHandler { get; init; }
    public List<CardOrderRank> TrumpOrder { get; init; } = [];
    public List<CardOrderRank> PlainSuitOrder { get; init; } = [];
}
