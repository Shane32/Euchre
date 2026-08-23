using System.Text.Json;
using System.Text.Json.Serialization;

namespace GameService.Serialization;

/// <summary>Converts enum values to and from camel-case JSON strings.</summary>
public sealed class CamelCaseJsonStringEnumConverter : JsonStringEnumConverter
{
    /// <summary>Initializes a new instance of the camel case JSON string enum converter type.</summary>
    public CamelCaseJsonStringEnumConverter()
        : base(JsonNamingPolicy.CamelCase, allowIntegerValues: false)
    {
    }
}
