namespace GameService.State;

public sealed class PhaseState
{
    public GamePhase Id { get; set; }
    public int Step { get; set; }
    public DateTimeOffset? StartedAt { get; set; }
}
