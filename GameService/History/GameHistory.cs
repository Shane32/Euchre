using System.Text.Json;

namespace GameService.History;

public sealed class GameHistory
{
    public string SchemaVersion { get; init; } = GameHistorySchemaVersions.Version1;
    public required string GameId { get; init; }
    public GameType GameType { get; init; }
    public required string RulesVersion { get; init; }
    public Guid BaseRandomSeed { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public List<PlayerRegistration> Players { get; init; } = [];
    public Dictionary<string, JsonElement> Options { get; init; } = new(StringComparer.Ordinal);
    public List<GameHistoryEntry> Entries { get; init; } = [];
}
