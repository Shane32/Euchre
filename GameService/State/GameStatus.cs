using System.Text.Json.Serialization;

namespace GameService.State;

[JsonConverter(typeof(CamelCaseJsonStringEnumConverter))]
public enum GameStatus
{
    Waiting,
    Active,
    Paused,
    Completed,
    Abandoned
}
