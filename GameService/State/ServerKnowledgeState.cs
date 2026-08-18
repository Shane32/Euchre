namespace GameService.State;

public sealed class ServerKnowledgeState
{
    public long Epoch { get; set; }
    public Dictionary<string, ViewerKnowledgeState> Viewers { get; set; } = new(StringComparer.Ordinal);
}
