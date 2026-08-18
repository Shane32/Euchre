using System.Text.Json;
using System.Text.Json.Serialization;

namespace GameService.Definitions;

public sealed class GameDefinition
{
    public required string SchemaVersion { get; init; }
    public GameType GameType { get; init; }
    public required string RulesVersion { get; init; }
    public required string DisplayName { get; init; }
    public required PlayerConfigurationDefinition PlayerConfiguration { get; init; }
    public Dictionary<string, OptionDefinition> Options { get; init; } = new(StringComparer.Ordinal);
    public required CardSetDefinition CardSet { get; init; }
    public List<PileDefinition> PileDefinitions { get; init; } = [];
    public List<StateVariableDefinition> StateVariables { get; init; } = [];
    public List<PhaseDefinition> Phases { get; init; } = [];
    public List<ActionDefinition> Actions { get; init; } = [];
    public CardRulesDefinition? CardRules { get; init; }
    public ScoringDefinition? Scoring { get; init; }
    public Dictionary<string, JsonElement> Extensions { get; init; } = new(StringComparer.Ordinal);

    [JsonExtensionData]
    public Dictionary<string, JsonElement>? AdditionalData { get; init; }
}
