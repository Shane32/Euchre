using System.Text.Json.Serialization;

namespace GameService.Cards;

/// <summary>Specifies the supported card construction kind values.</summary>
[JsonConverter(typeof(CamelCaseJsonStringEnumConverter))]
public enum CardConstructionKind
{
    /// <summary>Represents the cartesian product option.</summary>
    CartesianProduct,
    /// <summary>Represents the explicit option.</summary>
    Explicit
}
