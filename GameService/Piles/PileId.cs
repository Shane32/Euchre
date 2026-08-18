using System.Text.Json.Serialization;

namespace GameService.Piles;

[JsonConverter(typeof(PileIdJsonConverter))]
public readonly record struct PileId
{
    public PileId(PileType type, int? ownerId = null)
    {
        if (ownerId <= 0) {
            throw new ArgumentOutOfRangeException(nameof(ownerId), "A pile owner ID must be a positive integer when present.");
        }

        Type = type;
        OwnerId = ownerId;
    }

    public PileType Type { get; }

    public int? OwnerId { get; }

    public override string ToString() => Type switch {
        PileType.Deck => Format(PileNames.Deck),
        PileType.Stock => Format(PileNames.Stock),
        PileType.Waste => Format(PileNames.Waste),
        PileType.Hand => Format(PileNames.Hand),
        PileType.Kitty => Format(PileNames.Kitty),
        PileType.Widow => Format(PileNames.Widow),
        PileType.CurrentTrick => Format(PileNames.CurrentTrick),
        PileType.Won => Format(PileNames.Won),
        PileType.Capture => Format(PileNames.Capture),
        PileType.Tableau => Format(PileNames.Tableau),
        PileType.Foundation => Format(PileNames.Foundation),
        PileType.Meld => Format(PileNames.Meld),
        PileType.PassSelection => Format(PileNames.PassSelection),
        PileType.Discard => Format(PileNames.Discard),
        PileType.Reserve => Format(PileNames.Reserve),
        _ => throw new InvalidOperationException($"Unsupported pile type '{Type}'.")
    };

    private string Format(string name) => OwnerId.HasValue ? $"{name}:{OwnerId}" : name;
}
