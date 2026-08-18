using System.Text.Json.Serialization;

namespace GameService.Definitions;

[JsonConverter(typeof(CamelCaseJsonStringEnumConverter))]
public enum OptionValueType
{
    Boolean,
    Integer,
    String
}
