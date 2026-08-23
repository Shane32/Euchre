using System.Text.Json.Serialization;

namespace GameService.Definitions;

/// <summary>Specifies the supported visibility policy values.</summary>
[JsonConverter(typeof(CamelCaseJsonStringEnumConverter))]
public enum VisibilityPolicy
{
    /// <summary>Represents the none option.</summary>
    None,
    /// <summary>Represents the public option.</summary>
    Public,
    /// <summary>Represents the owner option.</summary>
    Owner,
    /// <summary>Represents the team option.</summary>
    Team,
    /// <summary>Represents the known option.</summary>
    Known,
    /// <summary>Represents the owner or known option.</summary>
    OwnerOrKnown,
    /// <summary>Represents the face up or known option.</summary>
    FaceUpOrKnown,
    /// <summary>Represents the visible card positions option.</summary>
    VisibleCardPositions
}
