using System.Text.Json;

namespace GameService.Definitions;

/// <summary>Defines option configuration.</summary>
public sealed class OptionDefinition
{
    /// <summary>Gets or sets the type.</summary>
    public OptionValueType Type { get; init; }
    /// <summary>Gets or sets the default.</summary>
    public JsonElement Default { get; init; }
    /// <summary>Gets or sets the minimum.</summary>
    public int? Minimum { get; init; }
    /// <summary>Gets or sets the maximum.</summary>
    public int? Maximum { get; init; }
    /// <summary>Gets or sets the enum.</summary>
    public List<string>? Enum { get; init; }
}
