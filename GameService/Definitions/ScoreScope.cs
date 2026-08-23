using System.Text.Json.Serialization;

namespace GameService.Definitions;

/// <summary>Specifies the supported score scope values.</summary>
[JsonConverter(typeof(CamelCaseJsonStringEnumConverter))]
public enum ScoreScope
{
    /// <summary>Represents the player option.</summary>
    Player,
    /// <summary>Represents the team option.</summary>
    Team
}
