using System.Text.Json;

namespace GameService.Piles;

/// <summary>Represents the current pile state.</summary>
public sealed class PileState
{
    /// <summary>Gets or sets the pile identifier.</summary>
    public PileId PileId { get; set; }
    /// <summary>Gets or sets the definition identifier.</summary>
    public PileType DefinitionId { get; set; }
    /// <summary>Gets or sets the kind.</summary>
    public PileKind Kind { get; set; }
    /// <summary>Gets or sets the owner.</summary>
    public PileOwnerState? Owner { get; set; }
    /// <summary>Gets or sets the count.</summary>
    public int? Count { get; set; }
    /// <summary>Gets or sets the cards.</summary>
    public List<CardState> Cards { get; set; } = [];
    /// <summary>Gets or sets the variables.</summary>
    public Dictionary<string, JsonElement> Variables { get; set; } = new(StringComparer.Ordinal);
}
