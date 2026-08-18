using System.Text.Json;

namespace GameService.Cards;

public sealed class ExplicitCardDefinition
{
    public required string CardId { get; init; }
    public CardRank? Rank { get; init; }
    public CardSuit? Suit { get; init; }
    public Dictionary<string, JsonElement> Metadata { get; init; } = new(StringComparer.Ordinal);
}
