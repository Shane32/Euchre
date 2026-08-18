using System.Text.Json;

namespace GameService.Cards;

public sealed class CardState
{
    public string? InstanceId { get; set; }
    public string? Ref { get; set; }
    public string? CardId { get; set; }
    public int? Copy { get; set; }
    public CardRank? Rank { get; set; }
    public CardSuit? Suit { get; set; }
    public bool FaceUp { get; set; }
    public bool Unknown { get; set; }
    public CardOrientation? Orientation { get; set; }
    public Dictionary<string, JsonElement> Annotations { get; set; } = new(StringComparer.Ordinal);
}
