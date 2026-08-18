namespace GameService.Players;

public sealed class PlayerState
{
    public required string PlayerId { get; set; }
    public int Seat { get; set; }
    public required string DisplayName { get; set; }
    public int? TeamId { get; set; }
    public PlayerRole Role { get; set; } = PlayerRole.Player;
    public bool Ready { get; set; }
    public ConnectionStatus Connection { get; set; } = ConnectionStatus.Connected;
    public PlayerControl Control { get; set; } = PlayerControl.Human;
}
