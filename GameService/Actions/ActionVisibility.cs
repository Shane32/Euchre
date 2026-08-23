using System.Text.Json.Serialization;

namespace GameService.Actions;

/// <summary>Specifies the supported action visibility values.</summary>
[JsonConverter(typeof(CamelCaseJsonStringEnumConverter))]
public enum ActionVisibility
{
    /// <summary>Represents the public option.</summary>
    Public,
    /// <summary>Represents the deal option.</summary>
    Deal,
    /// <summary>Represents the discard option.</summary>
    Discard
}
