using System.Text.Json;

namespace GameService.Definitions;

public sealed class StateVariableDefinition
{
    public required string Name { get; init; }
    public StateVariableType Type { get; init; }
    public JsonElement Default { get; init; }
    public bool Nullable { get; init; }
    public int? Minimum { get; init; }
    public int? Maximum { get; init; }
    public VisibilityPolicy Visibility { get; init; } = VisibilityPolicy.Public;
}
