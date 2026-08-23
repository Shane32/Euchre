namespace GameService.Games.Pinochle;

/// <summary>Represents the current pinochle meld state.</summary>
public sealed class PinochleMeldState
{
    /// <summary>Gets or sets the seat.</summary>
    public int Seat { get; set; }
    /// <summary>Gets or sets the team identifier.</summary>
    public int? TeamId { get; set; }
    /// <summary>Gets or sets the kind.</summary>
    public PinochleMeld Kind { get; set; }
    /// <summary>Gets or sets the card instance identifiers.</summary>
    public List<string> CardInstanceIds { get; set; } = [];
    /// <summary>Gets or sets the value.</summary>
    public int Value { get; set; }
    /// <summary>Gets or sets the revealed.</summary>
    public bool Revealed { get; set; }
}
