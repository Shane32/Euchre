namespace GameService.State;

/// <summary>Represents the current viewer knowledge state.</summary>
public sealed class ViewerKnowledgeState
{
    /// <summary>Gets or sets the known card instance identifiers.</summary>
    public HashSet<string> KnownCardInstanceIds { get; set; } = new(StringComparer.Ordinal);
    /// <summary>Gets or sets the card references.</summary>
    public Dictionary<string, string> CardRefs { get; set; } = new(StringComparer.Ordinal);
}
