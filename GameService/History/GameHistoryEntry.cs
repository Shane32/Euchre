using System.Text.Json;

namespace GameService.History;

/// <summary>Represents a game history entry.</summary>
public sealed class GameHistoryEntry
{
    /// <summary>Gets or sets the sequence.</summary>
    public long Sequence { get; init; }
    /// <summary>Gets or sets the revision.</summary>
    public long Revision { get; init; }
    /// <summary>Gets or sets the transport.</summary>
    public ActionTransport Transport { get; init; }
    /// <summary>Gets or sets the action.</summary>
    public GameActionType Action { get; init; }
    /// <summary>Gets or sets the action identifier.</summary>
    public required string ActionId { get; init; }
    /// <summary>Gets or sets the player identifier.</summary>
    public string? PlayerId { get; init; }
    /// <summary>Gets or sets the expected revision.</summary>
    public long? ExpectedRevision { get; init; }
    /// <summary>Gets or sets the arguments.</summary>
    public JsonElement? Arguments { get; init; }
    /// <summary>Gets or sets the source pile identifier.</summary>
    public PileId? SourcePileId { get; init; }
    /// <summary>Gets or sets the card index.</summary>
    public int? CardIndex { get; init; }
    /// <summary>Gets or sets the target pile identifier.</summary>
    public PileId? TargetPileId { get; init; }
    /// <summary>Gets or sets the random seeds.</summary>
    public List<Guid> RandomSeeds { get; init; } = [];
    /// <summary>Gets or sets the event timestamp.</summary>
    public DateTimeOffset OccurredAt { get; init; }
}
