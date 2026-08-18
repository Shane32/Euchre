
namespace GameService.Piles;

public static class PileIds
{
    public static readonly PileId Deck = new(PileType.Deck);
    public static readonly PileId Kitty = new(PileType.Kitty);
    public static readonly PileId CurrentTrick = new(PileType.CurrentTrick);

    public static PileId Hand(int seat) => new(PileType.Hand, seat);

    public static PileId Won(int team) => new(PileType.Won, team);

    public static PileId For(PileType type, int? ownerId = null) => new(type, ownerId);

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
