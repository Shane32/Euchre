namespace GameService.Actions;

public sealed class GameRuleException : InvalidOperationException
{
    public GameRuleException(GameRuleError code, string message)
        : base(message)
    {
        Code = code;
    }

    public GameRuleError Code { get; }
}
