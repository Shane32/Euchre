using System.Text.Json.Serialization;

namespace GameService.State;

[JsonConverter(typeof(CamelCaseJsonStringEnumConverter))]
public enum ExchangeKind
{
    Pass,
    Discard,
    Trade
}
