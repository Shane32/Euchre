using System.Text.Json;

namespace GameService.Cards;

/// <summary>Defines explicit card configuration.</summary>
public sealed class ExplicitCardDefinition
{
    /// <summary>Gets or sets the card identifier.</summary>
    public required string CardId { get; init; }
    /// <summary>Gets or sets the rank.</summary>
    public CardRank? Rank { get; init; }
    /// <summary>Gets or sets the suit.</summary>
    public CardSuit? Suit { get; init; }
    /// <summary>Gets or sets the metadata.</summary>
    public Dictionary<string, JsonElement> Metadata { get; init; } = new(StringComparer.Ordinal);
}
