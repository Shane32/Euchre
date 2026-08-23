using System.Text.Json;

namespace GameService.History;

/// <summary>Represents a game history.</summary>
public sealed class GameHistory
{
    /// <summary>Gets or sets the schema version.</summary>
    public string SchemaVersion { get; init; } = GameHistorySchemaVersions.Version1;
    /// <summary>Gets or sets the game identifier.</summary>
    public required string GameId { get; init; }
    /// <summary>Gets or sets the game type.</summary>
    public GameType GameType { get; init; }
    /// <summary>Gets or sets the rules version.</summary>
    public required string RulesVersion { get; init; }
    /// <summary>Gets or sets the base random seed.</summary>
    public Guid BaseRandomSeed { get; init; }
    /// <summary>Gets or sets the creation timestamp.</summary>
    public DateTimeOffset CreatedAt { get; init; }
    /// <summary>Gets or sets the players.</summary>
    public List<PlayerRegistration> Players { get; init; } = [];
    /// <summary>Gets or sets the options.</summary>
    public Dictionary<string, JsonElement> Options { get; init; } = new(StringComparer.Ordinal);
    /// <summary>Gets or sets the entries.</summary>
    public List<GameHistoryEntry> Entries { get; init; } = [];
}
