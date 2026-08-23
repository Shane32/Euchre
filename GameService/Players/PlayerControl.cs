using System.Text.Json.Serialization;

namespace GameService.Players;

/// <summary>Specifies the supported player control values.</summary>
[JsonConverter(typeof(CamelCaseJsonStringEnumConverter))]
public enum PlayerControl
{
    /// <summary>Represents the human option.</summary>
    Human,
    /// <summary>Represents the bot option.</summary>
    Bot,
    /// <summary>Represents the server option.</summary>
    Server
}
