using System.Text.Json.Serialization;

namespace GameService.State;

[JsonConverter(typeof(CamelCaseJsonStringEnumConverter))]
public enum ContractKind
{
    Euchre,
    Pinochle,
    Spades,
    Hearts
}
