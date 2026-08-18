
namespace GameService.Infrastructure;

internal sealed record RecordedAction(string Fingerprint, ActionResult Result);
