using System.Text.Json.Serialization;

namespace GameService.Cards;

/// <summary>Specifies the supported card color values.</summary>
[JsonConverter(typeof(CamelCaseJsonStringEnumConverter))]
public enum CardColor
{
    /// <summary>Represents the black color.</summary>
    Black,
    /// <summary>Represents the red color.</summary>
    Red
}
