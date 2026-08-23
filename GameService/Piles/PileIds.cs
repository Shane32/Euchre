
namespace GameService.Piles;

/// <summary>Provides standard pile identifiers.</summary>
public static class PileIds
{
    /// <summary>Identifies the deck.</summary>
    public static readonly PileId Deck = new(PileType.Deck);
    /// <summary>Identifies the kitty.</summary>
    public static readonly PileId Kitty = new(PileType.Kitty);
    /// <summary>Identifies the current trick.</summary>
    public static readonly PileId CurrentTrick = new(PileType.CurrentTrick);

    /// <summary>Creates a hand pile identifier for a seat.</summary>
    public static PileId Hand(int seat) => new(PileType.Hand, seat);

    /// <summary>Creates a won-cards pile identifier for a team.</summary>
    public static PileId Won(int team) => new(PileType.Won, team);

    /// <summary>Creates a pile identifier for an optional owner.</summary>
    public static PileId For(PileType type, int? ownerId = null) => new(type, ownerId);

    /// <summary>Parses a pile identifier from its string representation.</summary>
    public static PileId Parse(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        if (value == PileNames.Deck) {
            return Deck;
        }

        if (value == PileNames.Kitty) {
            return Kitty;
        }

        if (value == PileNames.CurrentTrick) {
            return CurrentTrick;
        }

        foreach (var type in Enum.GetValues<PileType>()) {
            var unowned = new PileId(type);
            if (value == unowned.ToString()) {
                return unowned;
            }

            var separator = value.LastIndexOf(':');
            if (separator > 0
                && value[..separator] == unowned.ToString()
                && int.TryParse(value[(separator + 1)..], out var ownerId)) {
                return new PileId(type, ownerId);
            }
        }
        throw new FormatException($"'{value}' is not a recognized pile ID.");
    }
}
