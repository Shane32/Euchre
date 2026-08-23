namespace GameService.State;

/// <summary>Represents the current phase state.</summary>
public sealed class PhaseState
{
    /// <summary>Gets or sets the identifier.</summary>
    public GamePhase Id { get; set; }
    /// <summary>Gets or sets the step.</summary>
    public int Step { get; set; }
    /// <summary>Gets or sets the start timestamp.</summary>
    public DateTimeOffset? StartedAt { get; set; }
}
