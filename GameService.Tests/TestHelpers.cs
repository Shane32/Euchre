
namespace GameService.Tests;

internal static class TestHelpers
{
    public static readonly PlayerRegistration[] PLAYERS =
    [
        new("p1", "North"),
        new("p2", "East"),
        new("p3", "South"),
        new("p4", "West")
    ];

    public static CardState Card(CardRank rank, CardSuit suit, string? id = null)
        => new() {
            InstanceId = id ?? $"{rank}.{suit}.{Guid.NewGuid():N}",
            CardId = $"{rank.ToString().ToLowerInvariant()}.{suit.ToString().ToLowerInvariant()}",
            Rank = rank,
            Suit = suit
        };

    public static PileState Hand(GameState state, int seat)
        => state.Piles.Single(pile => pile.PileId == PileIds.Hand(seat));

    public static PileState Pile(GameState state, PileId pileId)
        => state.Piles.Single(pile => pile.PileId == pileId);
}
