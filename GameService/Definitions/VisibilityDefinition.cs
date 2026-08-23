namespace GameService.Definitions;

/// <summary>Defines visibility configuration.</summary>
public sealed class VisibilityDefinition
{
    /// <summary>Gets or sets the existence.</summary>
    public VisibilityPolicy Existence { get; init; } = VisibilityPolicy.Public;
    /// <summary>Gets or sets the count.</summary>
    public VisibilityPolicy Count { get; init; } = VisibilityPolicy.Public;
    /// <summary>Gets or sets the cards.</summary>
    public VisibilityPolicy Cards { get; init; } = VisibilityPolicy.Public;
    /// <summary>Gets or sets the order.</summary>
    public VisibilityPolicy Order { get; init; } = VisibilityPolicy.Public;
}
