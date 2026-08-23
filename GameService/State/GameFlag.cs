using System.Text.Json.Serialization;

namespace GameService.State;

/// <summary>Specifies the supported game flag values.</summary>
[JsonConverter(typeof(CamelCaseJsonStringEnumConverter))]
public enum GameFlag
{
    /// <summary>Represents the first trick option.</summary>
    FirstTrick
}
