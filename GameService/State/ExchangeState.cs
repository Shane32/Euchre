namespace GameService.State;

/// <summary>Represents the current exchange state.</summary>
public sealed class ExchangeState
{
    /// <summary>Gets or sets the kind.</summary>
    public ExchangeKind Kind { get; set; }
    /// <summary>Gets or sets the direction.</summary>
    public ExchangeDirection? Direction { get; set; }
    /// <summary>Gets or sets the required count.</summary>
    public int RequiredCount { get; set; }
    /// <summary>Gets or sets the eligible seats.</summary>
    public List<int> EligibleSeats { get; set; } = [];
    /// <summary>Gets or sets the submitted seats.</summary>
    public List<int> SubmittedSeats { get; set; } = [];
    /// <summary>Gets or sets the complete.</summary>
    public bool Complete { get; set; }
    /// <summary>Gets or sets the public revealed.</summary>
    public bool PublicRevealed { get; set; }
}
