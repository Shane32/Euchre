using System.Text.Json.Serialization;

namespace GameService.Players;

/// <summary>Specifies the supported player role values.</summary>
[JsonConverter(typeof(CamelCaseJsonStringEnumConverter))]
public enum PlayerRole
{
    /// <summary>Represents the player player role.</summary>
    Player,
    /// <summary>Represents the spectator player role.</summary>
    Spectator
}
