using System.Text.Json;

namespace GameService.Actions;

public sealed record GameActionRequest(
    string PlayerId,
    GameActionType Name,
    JsonElement Arguments,
    string ActionId,
    long? ExpectedRevision = null);
