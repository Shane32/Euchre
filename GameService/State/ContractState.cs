using System.Text.Json;

namespace GameService.State;

public sealed class ContractState
{
    public ContractKind Kind { get; set; }
    public int DeclarerSeat { get; set; }
    public int? TeamId { get; set; }
    public int? Value { get; set; }
    public TrumpKind? TrumpKind { get; set; }
    public CardSuit? Suit { get; set; }
    public int? PartnerSeat { get; set; }
    public int? AloneSeat { get; set; }
    public Dictionary<string, JsonElement> Variables { get; set; } = new(StringComparer.Ordinal);
}
