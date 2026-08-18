using System.Text.Json;

namespace GameService.Actions;

public sealed class ActionDefinition
{
    public GameActionType Id { get; init; }
    public ActionTransport Transport { get; init; }
    public List<GamePhase> Phases { get; init; } = [];
    public ActorSelector Actor { get; init; }
    public JsonElement Arguments { get; init; }
    public required string ValidationHandler { get; init; }
    public required string ApplyHandler { get; init; }
    public ActionVisibility Visibility { get; init; }
}
