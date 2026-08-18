namespace GameService.Actions;

public sealed record PlayCardRequest(
    string PlayerId,
    PileId SourcePileId,
    int CardIndex,
    PileId TargetPileId,
    string ActionId,
    long? ExpectedRevision = null);
