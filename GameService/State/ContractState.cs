using System.Text.Json;

namespace GameService.State;

/// <summary>Represents the current contract state.</summary>
public sealed class ContractState
{
    /// <summary>Gets or sets the kind.</summary>
    public ContractKind Kind { get; set; }
    /// <summary>Gets or sets the declarer seat.</summary>
    public int DeclarerSeat { get; set; }
    /// <summary>Gets or sets the team identifier.</summary>
    public int? TeamId { get; set; }
    /// <summary>Gets or sets the value.</summary>
    public int? Value { get; set; }
    /// <summary>Gets or sets the trump kind.</summary>
    public TrumpKind? TrumpKind { get; set; }
    /// <summary>Gets or sets the suit.</summary>
    public CardSuit? Suit { get; set; }
    /// <summary>Gets or sets the partner seat.</summary>
    public int? PartnerSeat { get; set; }
    /// <summary>Gets or sets the alone seat.</summary>
    public int? AloneSeat { get; set; }
    /// <summary>Gets or sets the variables.</summary>
    public Dictionary<string, JsonElement> Variables { get; set; } = new(StringComparer.Ordinal);
}
