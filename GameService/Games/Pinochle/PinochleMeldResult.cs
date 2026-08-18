
namespace GameService.Games.Pinochle;

public sealed record PinochleMeldResult(int Total, IReadOnlyDictionary<PinochleMeld, int> Values);
