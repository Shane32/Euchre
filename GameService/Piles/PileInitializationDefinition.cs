namespace GameService.Piles;

public sealed class PileInitializationDefinition
{
    public PileCardSource? Cards { get; init; }
    public ShuffleAlgorithm? Shuffle { get; init; }
}
