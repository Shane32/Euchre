using System.Text.Json;

namespace GameService.Definitions;

public sealed class OptionDefinition
{
    public OptionValueType Type { get; init; }
    public JsonElement Default { get; init; }
    public int? Minimum { get; init; }
    public int? Maximum { get; init; }
    public List<string>? Enum { get; init; }
}
