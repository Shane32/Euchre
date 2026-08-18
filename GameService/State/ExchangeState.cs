namespace GameService.State;

public sealed class ExchangeState
{
    public ExchangeKind Kind { get; set; }
    public ExchangeDirection? Direction { get; set; }
    public int RequiredCount { get; set; }
    public List<int> EligibleSeats { get; set; } = [];
    public List<int> SubmittedSeats { get; set; } = [];
    public bool Complete { get; set; }
    public bool PublicRevealed { get; set; }
}
