using System.Text.Json.Serialization;

namespace GameService.Definitions;

[JsonConverter(typeof(CamelCaseJsonStringEnumConverter))]
public enum VisibilityPolicy
{
    None,
    Public,
    Owner,
    Team,
    Known,
    OwnerOrKnown,
    FaceUpOrKnown,
    VisibleCardPositions
}
