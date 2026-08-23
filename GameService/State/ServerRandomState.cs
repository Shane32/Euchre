namespace GameService.State;

/// <summary>Represents the current server random state.</summary>
public sealed class ServerRandomState
{
    /// <summary>Gets or sets the base seed.</summary>
    public Guid BaseSeed { get; set; }
    /// <summary>Gets or sets the current deal seed.</summary>
    public Guid? CurrentDealSeed { get; set; }
    /// <summary>Gets or sets the deal sequence.</summary>
    public int DealSequence { get; set; }
    /// <summary>Gets or sets the shuffle number.</summary>
    public int ShuffleNumber { get; set; }
}
