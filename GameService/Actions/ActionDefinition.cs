using System.Text.Json;

namespace GameService.Actions;

/// <summary>Defines action configuration.</summary>
public sealed class ActionDefinition
{
    /// <summary>Gets or sets the identifier.</summary>
    public GameActionType Id { get; init; }
    /// <summary>Gets or sets the transport.</summary>
    public ActionTransport Transport { get; init; }
    /// <summary>Gets or sets the phases.</summary>
    public List<GamePhase> Phases { get; init; } = [];
    /// <summary>Gets or sets the actor.</summary>
    public ActorSelector Actor { get; init; }
    /// <summary>Gets or sets the arguments.</summary>
    public JsonElement Arguments { get; init; }
    /// <summary>Gets or sets the validation handler.</summary>
    public required string ValidationHandler { get; init; }
    /// <summary>Gets or sets the apply handler.</summary>
    public required string ApplyHandler { get; init; }
    /// <summary>Gets or sets the visibility.</summary>
    public ActionVisibility Visibility { get; init; }
}
