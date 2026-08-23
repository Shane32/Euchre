using System.Text.Json.Serialization;

namespace GameService.Cards;

/// <summary>Specifies the supported card orientation values.</summary>
[JsonConverter(typeof(CamelCaseJsonStringEnumConverter))]
public enum CardOrientation
{
    /// <summary>Represents the upright option.</summary>
    Upright,
    /// <summary>Represents the sideways option.</summary>
    Sideways,
    /// <summary>Represents the inverted option.</summary>
    Inverted
}
