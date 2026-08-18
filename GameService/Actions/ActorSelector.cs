using System.Text.Json.Serialization;

namespace GameService.Actions;

[JsonConverter(typeof(CamelCaseJsonStringEnumConverter))]
public enum ActorSelector
{
    Self,
    AllPlayers,
    ActiveSeat,
    Dealer,
    Declarer,
    System,
    None
}
