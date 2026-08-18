namespace GameService.State;

public sealed class ServerRandomState
{
    public Guid BaseSeed { get; set; }
    public Guid? CurrentDealSeed { get; set; }
    public int DealSequence { get; set; }
    public int ShuffleNumber { get; set; }
}
