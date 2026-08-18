namespace GameService.State;

public sealed class BidState
{
    public int Seat { get; set; }
    public BidKind Kind { get; set; }
    public int? Value { get; set; }
    public CardSuit? Suit { get; set; }
    public bool Alone { get; set; }
    public long Revision { get; set; }
}
