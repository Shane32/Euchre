
namespace GameService.Games.Pinochle;

/// <summary>Represents the result of pinochle meld.</summary>
public sealed record PinochleMeldResult(int Total, IReadOnlyDictionary<PinochleMeld, int> Values);
