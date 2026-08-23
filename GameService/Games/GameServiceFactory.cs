using GameService.Games.Euchre;

namespace GameService.Games;

/// <summary>Represents a game service factory.</summary>
public static class GameServiceFactory
{
    /// <summary>Restores a game service from recorded history.</summary>
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
