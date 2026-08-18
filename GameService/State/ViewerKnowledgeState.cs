namespace GameService.State;

public sealed class ViewerKnowledgeState
{
    public HashSet<string> KnownCardInstanceIds { get; set; } = new(StringComparer.Ordinal);
    public Dictionary<string, string> CardRefs { get; set; } = new(StringComparer.Ordinal);
}
