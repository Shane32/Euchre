namespace GameService.Players;

/// <summary>Represents the current player state.</summary>
public sealed class PlayerState
{
    /// <summary>Gets or sets the player identifier.</summary>
    public required string PlayerId { get; set; }
    /// <summary>Gets or sets the seat.</summary>
    public int Seat { get; set; }
    /// <summary>Gets or sets the display name.</summary>
    public required string DisplayName { get; set; }
    /// <summary>Gets or sets the team identifier.</summary>
    public int? TeamId { get; set; }
    /// <summary>Gets or sets the role.</summary>
    public PlayerRole Role { get; set; } = PlayerRole.Player;
    /// <summary>Gets or sets the ready.</summary>
    public bool Ready { get; set; }
    /// <summary>Gets or sets the connection.</summary>
    public ConnectionStatus Connection { get; set; } = ConnectionStatus.Connected;
    /// <summary>Gets or sets the control.</summary>
    public PlayerControl Control { get; set; } = PlayerControl.Human;
}
