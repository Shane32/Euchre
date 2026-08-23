using System.Text.Json.Serialization;

namespace GameService.State;

/// <summary>Specifies the supported game type values.</summary>
[JsonConverter(typeof(CamelCaseJsonStringEnumConverter))]
public enum GameType
{
    /// <summary>Represents the euchre game type.</summary>
    Euchre,
    /// <summary>Represents the pinochle game type.</summary>
    Pinochle
}
