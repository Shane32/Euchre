namespace GameService.Actions;

/// <summary>Represents a game rule failure.</summary>
public sealed class GameRuleException : InvalidOperationException
{
    /// <summary>Initializes a new instance of the game rule exception type.</summary>
    public GameRuleException(GameRuleError code, string message)
        : base(message)
    {
        Code = code;
    }

    /// <summary>Gets the code.</summary>
    public GameRuleError Code { get; }
}
