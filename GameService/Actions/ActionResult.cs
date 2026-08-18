namespace GameService.Actions;

public sealed record ActionResult(
    bool Accepted,
    string ActionId,
    long Revision,
    GameRuleError? Error = null);
