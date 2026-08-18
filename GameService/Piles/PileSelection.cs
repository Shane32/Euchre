using System.Text.Json.Serialization;

namespace GameService.Piles;

[JsonConverter(typeof(CamelCaseJsonStringEnumConverter))]
public enum PileSelection
{
    None,
    Top,
    Bottom,
    Any,
    EuchreKitty
}
