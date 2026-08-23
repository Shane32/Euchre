namespace GameService.Actions;

/// <summary>Represents a play card request.</summary>
public sealed record PlayCardRequest(
    string PlayerId,
    PileId SourcePileId,
    int CardIndex,
    PileId TargetPileId,
    string ActionId,
    long? ExpectedRevision = null);
