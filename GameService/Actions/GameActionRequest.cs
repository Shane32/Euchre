using System.Text.Json;

namespace GameService.Actions;

/// <summary>Represents a game action request.</summary>
public sealed record GameActionRequest(
    string PlayerId,
    GameActionType Name,
    JsonElement Arguments,
    string ActionId,
    long? ExpectedRevision = null);
