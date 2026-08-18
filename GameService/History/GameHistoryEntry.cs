using System.Text.Json;

namespace GameService.History;

public sealed class GameHistoryEntry
{
    public long Sequence { get; init; }
    public long Revision { get; init; }
    public ActionTransport Transport { get; init; }
    public GameActionType Action { get; init; }
    public required string ActionId { get; init; }
    public string? PlayerId { get; init; }
    public long? ExpectedRevision { get; init; }
    public JsonElement? Arguments { get; init; }
    public PileId? SourcePileId { get; init; }
    public int? CardIndex { get; init; }
    public PileId? TargetPileId { get; init; }
    public List<Guid> RandomSeeds { get; init; } = [];
    public DateTimeOffset OccurredAt { get; init; }
}
