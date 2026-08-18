using System.Text.Json;

namespace GameService.Piles;

public sealed class PileDefinition
{
    public PileType Id { get; init; }
    public PileKind Kind { get; init; }
    public PileInstantiation Instantiate { get; init; }
    public PileOwnerKind? Owner { get; init; }
    public bool Ordered { get; init; }
    public PileTop? Top { get; init; }
    public PileSelection Select { get; init; }
    public required VisibilityDefinition Visibility { get; init; }
    public PileInitializationDefinition? Initialization { get; init; }
    public Dictionary<string, JsonElement> Metadata { get; init; } = new(StringComparer.Ordinal);
}
