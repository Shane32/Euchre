namespace GameService.State;

/// <summary>Represents the current server knowledge state.</summary>
public sealed class ServerKnowledgeState
{
    /// <summary>Gets or sets the epoch.</summary>
    public long Epoch { get; set; }
    /// <summary>Gets or sets the viewers.</summary>
    public Dictionary<string, ViewerKnowledgeState> Viewers { get; set; } = new(StringComparer.Ordinal);
}
