using System.Text.Json;

namespace GameService.Definitions;

/// <summary>Defines state variable configuration.</summary>
public sealed class StateVariableDefinition
{
    /// <summary>Gets or sets the name.</summary>
    public required string Name { get; init; }
    /// <summary>Gets or sets the type.</summary>
    public StateVariableType Type { get; init; }
    /// <summary>Gets or sets the default.</summary>
    public JsonElement Default { get; init; }
    /// <summary>Gets or sets the nullable.</summary>
    public bool Nullable { get; init; }
    /// <summary>Gets or sets the minimum.</summary>
    public int? Minimum { get; init; }
    /// <summary>Gets or sets the maximum.</summary>
    public int? Maximum { get; init; }
    /// <summary>Gets or sets the visibility.</summary>
    public VisibilityPolicy Visibility { get; init; } = VisibilityPolicy.Public;
}
