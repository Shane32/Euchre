using System.Text.Json;

namespace GameService.Piles;

public sealed class PileState
{
    public PileId PileId { get; set; }
    public PileType DefinitionId { get; set; }
    public PileKind Kind { get; set; }
    public PileOwnerState? Owner { get; set; }
    public int? Count { get; set; }
    public List<CardState> Cards { get; set; } = [];
    public Dictionary<string, JsonElement> Variables { get; set; } = new(StringComparer.Ordinal);
}
