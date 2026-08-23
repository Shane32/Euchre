namespace GameService.Piles;

/// <summary>Defines pile initialization configuration.</summary>
public sealed class PileInitializationDefinition
{
    /// <summary>Gets or sets the cards.</summary>
    public PileCardSource? Cards { get; init; }
    /// <summary>Gets or sets the shuffle.</summary>
    public ShuffleAlgorithm? Shuffle { get; init; }
}
