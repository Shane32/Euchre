namespace GameService.Definitions;

public sealed class VisibilityDefinition
{
    public VisibilityPolicy Existence { get; init; } = VisibilityPolicy.Public;
    public VisibilityPolicy Count { get; init; } = VisibilityPolicy.Public;
    public VisibilityPolicy Cards { get; init; } = VisibilityPolicy.Public;
    public VisibilityPolicy Order { get; init; } = VisibilityPolicy.Public;
}
