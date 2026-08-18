using System.Text.Json;

namespace GameService.Infrastructure;

internal sealed class PendingHistoryAction
{
    public ActionTransport Transport { get; init; }
    public GameActionType Action { get; set; }
    public string? PlayerId { get; init; }
    public long? ExpectedRevision { get; init; }
    public JsonElement? Arguments { get; init; }
    public PileId? SourcePileId { get; init; }
    public int? CardIndex { get; init; }
    public PileId? TargetPileId { get; init; }
}
