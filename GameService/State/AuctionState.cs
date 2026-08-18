namespace GameService.State;

public sealed class AuctionState
{
    public int Round { get; set; }
    public int? ActiveSeat { get; set; }
    public List<BidState> Calls { get; set; } = [];
    public BidState? HighCall { get; set; }
    public List<int> PassedSeats { get; set; } = [];
    public bool Complete { get; set; }
}
