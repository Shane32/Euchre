using System.Text.Json.Serialization;

namespace GameService.Definitions;

/// <summary>Specifies the supported option value type values.</summary>
[JsonConverter(typeof(CamelCaseJsonStringEnumConverter))]
public enum OptionValueType
{
    /// <summary>Represents the boolean option.</summary>
    Boolean,
    /// <summary>Represents the integer option.</summary>
    Integer,
    /// <summary>Represents the string option.</summary>
    String
}
