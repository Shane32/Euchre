using System.Text.Json.Serialization;

namespace GameService.State;

/// <summary>Specifies the supported game status values.</summary>
[JsonConverter(typeof(CamelCaseJsonStringEnumConverter))]
public enum GameStatus
{
    /// <summary>Represents the waiting game status.</summary>
    Waiting,
    /// <summary>Represents the active game status.</summary>
    Active,
    /// <summary>Represents the paused game status.</summary>
    Paused,
    /// <summary>Represents the completed game status.</summary>
    Completed,
    /// <summary>Represents the abandoned game status.</summary>
    Abandoned
}
