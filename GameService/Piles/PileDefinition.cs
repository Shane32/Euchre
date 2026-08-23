using System.Text.Json;

namespace GameService.Piles;

/// <summary>Defines pile configuration.</summary>
public sealed class PileDefinition
{
    /// <summary>Gets or sets the identifier.</summary>
    public PileType Id { get; init; }
    /// <summary>Gets or sets the kind.</summary>
    public PileKind Kind { get; init; }
    /// <summary>Gets or sets the instantiate.</summary>
    public PileInstantiation Instantiate { get; init; }
    /// <summary>Gets or sets the owner.</summary>
    public PileOwnerKind? Owner { get; init; }
    /// <summary>Gets or sets the ordered.</summary>
    public bool Ordered { get; init; }
    /// <summary>Gets or sets the top.</summary>
    public PileTop? Top { get; init; }
    /// <summary>Gets or sets the select.</summary>
    public PileSelection Select { get; init; }
    /// <summary>Gets or sets the visibility.</summary>
    public required VisibilityDefinition Visibility { get; init; }
    /// <summary>Gets or sets the initialization.</summary>
    public PileInitializationDefinition? Initialization { get; init; }
    /// <summary>Gets or sets the metadata.</summary>
    public Dictionary<string, JsonElement> Metadata { get; init; } = new(StringComparer.Ordinal);
}
