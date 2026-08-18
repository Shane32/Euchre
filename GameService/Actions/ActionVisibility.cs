using System.Text.Json.Serialization;

namespace GameService.Actions;

[JsonConverter(typeof(CamelCaseJsonStringEnumConverter))]
public enum ActionVisibility
{
    Public,
    Deal,
    Discard
}
