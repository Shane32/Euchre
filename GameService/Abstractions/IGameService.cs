
namespace GameService.Abstractions;

public interface IGameService
{
    GameDefinition Definition { get; }
    GameHistory GetHistory();
    GameState GetGameState(string playerId);
    string GetGameHash(string playerId);
    IReadOnlyList<GameActionType> GetLegalActions(string playerId);
    void StartGame();
    ActionResult PlayCard(PlayCardRequest request);
    ActionResult PerformAction(GameActionRequest request);
}
