using System.Text.Json.Serialization;

namespace GameService.State;

/// <summary>Specifies the supported game result reason values.</summary>
[JsonConverter(typeof(CamelCaseJsonStringEnumConverter))]
public enum GameResultReason
{
    /// <summary>Represents the target score option.</summary>
    TargetScore
}
