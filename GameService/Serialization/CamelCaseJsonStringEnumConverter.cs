using System.Text.Json;
using System.Text.Json.Serialization;

namespace GameService.Serialization;

public sealed class CamelCaseJsonStringEnumConverter : JsonStringEnumConverter
{
    public CamelCaseJsonStringEnumConverter()
        : base(JsonNamingPolicy.CamelCase, allowIntegerValues: false)
    {
    }
}
