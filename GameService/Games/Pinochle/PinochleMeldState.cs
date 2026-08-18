namespace GameService.Games.Pinochle;

public sealed class PinochleMeldState
{
    public int Seat { get; set; }
    public int? TeamId { get; set; }
    public PinochleMeld Kind { get; set; }
    public List<string> CardInstanceIds { get; set; } = [];
    public int Value { get; set; }
    public bool Revealed { get; set; }
}
