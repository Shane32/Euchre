namespace GameService.Piles;

public sealed class PileOwnerState
{
    public PileOwnerKind Kind { get; set; }
    public int? Seat { get; set; }
    public int? Team { get; set; }
}
