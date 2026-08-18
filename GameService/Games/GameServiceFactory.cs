using GameService.Games.Euchre;

namespace GameService.Games;

public static class GameServiceFactory
{
    public static IGameService Restore(GameHistory history)
    {
        ArgumentNullException.ThrowIfNull(history);
        return history.GameType switch {
            GameType.Euchre => EuchreGameService.Restore(history),
            GameType.Pinochle => PinochleGameService.Restore(history),
            _ => throw new ArgumentOutOfRangeException(nameof(history), history.GameType, "Unsupported game type.")
        };
    }
}
