using System.Text.Json.Serialization;

namespace GameService.Actions;

/// <summary>Specifies the supported actor selector values.</summary>
[JsonConverter(typeof(CamelCaseJsonStringEnumConverter))]
public enum ActorSelector
{
    /// <summary>Represents the self option.</summary>
    Self,
    /// <summary>Represents the all players option.</summary>
    AllPlayers,
    /// <summary>Represents the active seat option.</summary>
    ActiveSeat,
    /// <summary>Represents the dealer option.</summary>
    Dealer,
    /// <summary>Represents the declarer option.</summary>
    Declarer,
    /// <summary>Represents the system option.</summary>
    System,
    /// <summary>Represents the none option.</summary>
    None
}
