using System.Text.Json;

namespace GameService.Definitions;

public static class GameDefinitionLoader
{
    private static readonly JsonSerializerOptions s_options = new() {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters =
        {
            new CardSymbolJsonConverter(),
            new System.Text.Json.Serialization.JsonStringEnumConverter(JsonNamingPolicy.CamelCase, allowIntegerValues: false)
        }
    };

    public static GameDefinition LoadEmbedded(GameType gameType)
    {
        var assembly = typeof(GameDefinitionLoader).Assembly;
        var suffix = $".Games.{gameType}.{gameType.ToString().ToLowerInvariant()}.json";
        var resourceName = assembly.GetManifestResourceNames()
            .SingleOrDefault(name => name.EndsWith(suffix, StringComparison.OrdinalIgnoreCase))
            ?? throw new InvalidOperationException($"Embedded game definition '{gameType}' was not found.");

        using var stream = assembly.GetManifestResourceStream(resourceName)
            ?? throw new InvalidOperationException($"Embedded resource '{resourceName}' could not be opened.");
        var definition = JsonSerializer.Deserialize<GameDefinition>(stream, s_options)
            ?? throw new InvalidOperationException($"Embedded game definition '{gameType}' was empty.");
        Validate(definition);
        return definition;
    }

    public static void Validate(GameDefinition definition)
    {
        ArgumentNullException.ThrowIfNull(definition);
        if (definition.SchemaVersion != GameSchemaVersions.Version1) {
            throw new InvalidOperationException($"Unsupported schema version '{definition.SchemaVersion}'.");
        }

        var seats = definition.PlayerConfiguration.Seats;
        if (seats.Count != seats.Distinct().Count()) {
            throw new InvalidOperationException("Seat IDs must be unique.");
        }
        if (seats.Any(seat => seat <= 0)) {
            throw new InvalidOperationException("Seat IDs must be positive integers.");
        }
        if (definition.PlayerConfiguration.TurnOrder.Count != seats.Count
            || !definition.PlayerConfiguration.TurnOrder.ToHashSet().SetEquals(seats)) {
            throw new InvalidOperationException("Turn order must contain each configured seat exactly once.");
        }

        var teams = definition.PlayerConfiguration.Teams;
        if (teams.Select(team => team.TeamId).Distinct().Count() != teams.Count
            || teams.Any(team => team.TeamId <= 0)) {
            throw new InvalidOperationException("Team IDs must be unique positive integers.");
        }
        if (teams.SelectMany(team => team.SeatIds).Any(seat => !seats.Contains(seat))) {
            throw new InvalidOperationException("Every team seat must be a configured seat.");
        }

        var phaseIds = definition.Phases.Select(phase => phase.Id).ToHashSet();
        var actionIds = definition.Actions.Select(action => action.Id).ToHashSet();
        if (phaseIds.Count != definition.Phases.Count || actionIds.Count != definition.Actions.Count) {
            throw new InvalidOperationException("Phase IDs and action IDs must be unique.");
        }

        foreach (var phase in definition.Phases) {
            var unknown = phase.AllowedActions.Where(action => !actionIds.Contains(action)).ToArray();
            if (unknown.Length > 0) {
                throw new InvalidOperationException($"Phase '{phase.Id}' refers to unknown action '{unknown[0]}'.");
            }
        }

        foreach (var action in definition.Actions) {
            var unknown = action.Phases.Where(phase => !phaseIds.Contains(phase)).ToArray();
            if (unknown.Length > 0) {
                throw new InvalidOperationException($"Action '{action.Id}' refers to unknown phase '{unknown[0]}'.");
            }
        }

        if (definition.PileDefinitions.Select(pile => pile.Id).Distinct().Count()
            != definition.PileDefinitions.Count) {
            throw new InvalidOperationException("Pile definition IDs must be unique.");
        }
    }
}
