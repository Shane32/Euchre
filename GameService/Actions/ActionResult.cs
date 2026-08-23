namespace GameService.Actions;

/// <summary>Represents the result of a requested game action.</summary>
public sealed record ActionResult(
    bool Accepted,
    string ActionId,
    long Revision,
    GameRuleError? Error = null);
