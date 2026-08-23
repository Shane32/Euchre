using System.Text.Json.Serialization;

namespace GameService.State;

/// <summary>Specifies the supported score counter values.</summary>
[JsonConverter(typeof(CamelCaseJsonStringEnumConverter))]
public enum ScoreCounter
{
    /// <summary>Represents the card points option.</summary>
    CardPoints
}
