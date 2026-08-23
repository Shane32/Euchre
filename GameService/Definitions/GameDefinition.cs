using System.Text.Json;
using System.Text.Json.Serialization;

namespace GameService.Definitions;

/// <summary>Defines game configuration.</summary>
public sealed class GameDefinition
{
    /// <summary>Gets or sets the schema version.</summary>
    public required string SchemaVersion { get; init; }
    /// <summary>Gets or sets the game type.</summary>
    public GameType GameType { get; init; }
    /// <summary>Gets or sets the rules version.</summary>
    public required string RulesVersion { get; init; }
    /// <summary>Gets or sets the display name.</summary>
    public required string DisplayName { get; init; }
    /// <summary>Gets or sets the player configuration.</summary>
    public required PlayerConfigurationDefinition PlayerConfiguration { get; init; }
    /// <summary>Gets or sets the options.</summary>
    public Dictionary<string, OptionDefinition> Options { get; init; } = new(StringComparer.Ordinal);
    /// <summary>Gets or sets the card set.</summary>
    public required CardSetDefinition CardSet { get; init; }
    /// <summary>Gets or sets the pile definitions.</summary>
    public List<PileDefinition> PileDefinitions { get; init; } = [];
    /// <summary>Gets or sets the state variables.</summary>
    public List<StateVariableDefinition> StateVariables { get; init; } = [];
    /// <summary>Gets or sets the phases.</summary>
    public List<PhaseDefinition> Phases { get; init; } = [];
    /// <summary>Gets or sets the actions.</summary>
    public List<ActionDefinition> Actions { get; init; } = [];
    /// <summary>Gets or sets the card rules.</summary>
    public CardRulesDefinition? CardRules { get; init; }
    /// <summary>Gets or sets the scoring.</summary>
    public ScoringDefinition? Scoring { get; init; }
    /// <summary>Gets or sets the extensions.</summary>
    public Dictionary<string, JsonElement> Extensions { get; init; } = new(StringComparer.Ordinal);

    /// <summary>Gets or sets the additional data.</summary>
    [JsonExtensionData]
    public Dictionary<string, JsonElement>? AdditionalData { get; init; }
}
