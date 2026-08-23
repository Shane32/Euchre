
namespace GameService.Abstractions;

/// <summary>Defines operations for managing a card game.</summary>
public interface IGameService
{
    /// <summary>Gets the definition.</summary>
    GameDefinition Definition { get; }
    /// <summary>Gets the recorded game history.</summary>
    GameHistory GetHistory();
    /// <summary>Gets the game state visible to a player.</summary>
    GameState GetGameState(string playerId);
    /// <summary>Gets a stable hash of the game state visible to a player.</summary>
    string GetGameHash(string playerId);
    /// <summary>Gets the actions currently available to a player.</summary>
    IReadOnlyList<GameActionType> GetLegalActions(string playerId);
    /// <summary>Starts the game.</summary>
    void StartGame();
    /// <summary>Attempts to play a card.</summary>
    ActionResult PlayCard(PlayCardRequest request);
    /// <summary>Attempts to perform a game action.</summary>
    ActionResult PerformAction(GameActionRequest request);
}
