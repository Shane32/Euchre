using System.Text.Json;

namespace GameService.Tests;

public sealed class SchemaAndBaseTests
{
    [Fact]
    public void EmbeddedEuchreDefinitionDeserializesToModels()
    {
        var definition = GameDefinitionLoader.LoadEmbedded(GameType.Euchre);

        Assert.Equal(GameType.Euchre, definition.GameType);
        Assert.Equal(TableSeats.All, definition.PlayerConfiguration.Seats);
        Assert.Equal(TableTeams.All, definition.PlayerConfiguration.Teams.Select(team => team.TeamId));
        Assert.Equal(24, definition.CardSet.Suits.Count * definition.CardSet.Ranks.Count * definition.CardSet.Copies);
        Assert.Contains(definition.PileDefinitions, pile => pile.Id == PileType.Hand);
        Assert.Contains(definition.Actions, action => action.Id == GameActionType.PlayCard);
        Assert.Equal(CardSuit.Clubs, definition.CardSet.Suits[0].Id);
        Assert.Equal(CardSymbol.Clubs, definition.CardSet.Suits[0].Symbol);
        Assert.Equal(CardColor.Black, definition.CardSet.Suits[0].Color);
        Assert.Equal(CardRank.Nine, definition.CardSet.Ranks[0].Id);
        Assert.Equal(CardSymbol.Nine, definition.CardSet.Ranks[0].Symbol);
        Assert.Equal(PileKind.Stock, definition.PileDefinitions[0].Kind);
        Assert.Equal(PhaseMode.ExclusiveTurn, definition.Phases.Single(phase => phase.Id == GamePhase.Play).Mode);
        Assert.Equal(ActionTransport.PlayCard, definition.Actions.Single(action => action.Id == GameActionType.PlayCard).Transport);
    }

    [Fact]
    public void EmbeddedPinochleDefinitionDeserializesToModels()
    {
        var definition = GameDefinitionLoader.LoadEmbedded(GameType.Pinochle);

        Assert.Equal(GameType.Pinochle, definition.GameType);
        Assert.Equal(48, definition.CardSet.Suits.Count * definition.CardSet.Ranks.Count * definition.CardSet.Copies);
        Assert.Equal(2, definition.CardSet.Copies);
        Assert.Contains(definition.Actions, action => action.Id == GameActionType.Bid);
    }

    [Fact]
    public void StrongTypesSerializeAsSchemaCompatibleStringsByDefault()
    {
        Assert.Equal("\"hearts\"", JsonSerializer.Serialize(CardSuit.Hearts));
        Assert.Equal("\"ace\"", JsonSerializer.Serialize(CardRank.Ace));
        Assert.Equal("\"10\"", JsonSerializer.Serialize(CardSymbol.Ten));
        Assert.Equal("\"exclusiveTurn\"", JsonSerializer.Serialize(PhaseMode.ExclusiveTurn));
        Assert.Equal("\"playCard\"", JsonSerializer.Serialize(GameActionType.PlayCard));
        Assert.Equal("\"dealerDiscard\"", JsonSerializer.Serialize(GamePhase.DealerDiscard));
        Assert.Equal("\"hand:4\"", JsonSerializer.Serialize(PileIds.Hand(TableSeats.West)));
    }

    [Fact]
    public void OwnedPileIdsRequirePositiveNumericOwners()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => PileIds.Won(0));
        Assert.Equal(new PileId(PileType.Tableau, TableSeats.North), PileIds.Parse("tableau:1"));
    }

    [Fact]
    public void BaseModelInstantiatesSchemaPiles()
    {
        var game = new EuchreGameService("g1", TestHelpers.PLAYERS, seed: TestSeeds.FromInt32(1));
        game.StartGame();
        var state = game.GetGameState("p1");

        Assert.Contains(state.Piles, pile => pile.PileId == PileIds.Deck);
        Assert.Equal(4, state.Piles.Count(pile => pile.Kind == PileKind.Hand));
        Assert.Equal(2, state.Piles.Count(pile => pile.Kind == PileKind.Capture));
        Assert.Contains(state.Piles, pile => pile.PileId == PileIds.Kitty);
    }

    [Fact]
    public void SameSeedReproducesSameDeal()
    {
        var first = new EuchreGameService("g1", TestHelpers.PLAYERS, seed: TestSeeds.FromInt32(8675309));
        var second = new EuchreGameService("g2", TestHelpers.PLAYERS, seed: TestSeeds.FromInt32(8675309));
        first.StartGame();
        second.StartGame();

        foreach (var player in TestHelpers.PLAYERS) {
            var firstCards = TestHelpers.Hand(first.GetGameState(player.PlayerId), Seat(player.PlayerId)).Cards.Select(card => card.CardId);
            var secondCards = TestHelpers.Hand(second.GetGameState(player.PlayerId), Seat(player.PlayerId)).Cards.Select(card => card.CardId);
            Assert.Equal(firstCards, secondCards);
        }
    }

    [Fact]
    public void DifferentSeedsProduceDifferentDeals()
    {
        var first = new EuchreGameService("g1", TestHelpers.PLAYERS, seed: TestSeeds.FromInt32(1));
        var second = new EuchreGameService("g2", TestHelpers.PLAYERS, seed: TestSeeds.FromInt32(2));
        first.StartGame();
        second.StartGame();

        var firstCards = TestHelpers.Hand(first.GetGameState("p1"), TableSeats.North).Cards.Select(card => card.CardId).ToArray();
        var secondCards = TestHelpers.Hand(second.GetGameState("p1"), TableSeats.North).Cards.Select(card => card.CardId).ToArray();
        Assert.NotEqual(firstCards, secondCards);
    }

    [Fact]
    public void PlayerSeesOwnHandButNotOpponentsHand()
    {
        var game = new EuchreGameService("g1", TestHelpers.PLAYERS, seed: TestSeeds.FromInt32(10));
        game.StartGame();
        var state = game.GetGameState("p1");

        Assert.All(TestHelpers.Hand(state, TableSeats.North).Cards, card => Assert.False(card.Unknown));
        Assert.All(TestHelpers.Hand(state, TableSeats.East).Cards, card => Assert.True(card.Unknown));
        Assert.Single(TestHelpers.Pile(state, PileIds.Kitty).Cards, card => !card.Unknown);
    }

    [Fact]
    public void GetGameStateRequiresARealPlayer()
    {
        var game = new EuchreGameService("g1", TestHelpers.PLAYERS, seed: TestSeeds.FromInt32(1));
        var error = Assert.Throws<GameRuleException>(() => game.GetGameState("not-a-player"));
        Assert.Equal(GameRuleError.PlayerNotFound, error.Code);
    }

    [Fact]
    public void ActionIdsAreIdempotent()
    {
        var game = new EuchreGameService("g1", TestHelpers.PLAYERS, seed: TestSeeds.FromInt32(1));
        game.StartGame();
        var revision = game.GetGameState("p2").Revision;

        var first = game.Pass("p2", "pass-1", revision);
        var retry = game.Pass("p2", "pass-1", revision);

        Assert.Equal(first, retry);
        Assert.Equal(first.Revision, game.GetGameState("p1").Revision);
    }

    [Fact]
    public void ReusingActionIdWithDifferentContentIsRejected()
    {
        var game = new EuchreGameService("g1", TestHelpers.PLAYERS, seed: TestSeeds.FromInt32(1));
        game.StartGame();
        game.Pass("p2", "same-id");

        var error = Assert.Throws<GameRuleException>(() => game.OrderUp("p2", false, "same-id"));
        Assert.Equal(GameRuleError.ActionIdConflict, error.Code);
    }

    [Fact]
    public void StaleExpectedRevisionIsRejected()
    {
        var game = new EuchreGameService("g1", TestHelpers.PLAYERS, seed: TestSeeds.FromInt32(1));
        game.StartGame();

        var error = Assert.Throws<GameRuleException>(() => game.Pass("p2", "pass-1", expectedRevision: 0));
        Assert.Equal(GameRuleError.RevisionConflict, error.Code);
    }

    [Fact]
    public void HashIsStableForUnchangedViewerState()
    {
        var game = new EuchreGameService("g1", TestHelpers.PLAYERS, seed: TestSeeds.FromInt32(1));
        game.StartGame();
        Assert.Equal(game.GetGameHash("p1"), game.GetGameHash("p1"));
    }

    [Theory]
    [InlineData("targetScore", 0)]
    [InlineData("targetScore", 22)]
    [InlineData("stickTheDealer", "yes")]
    public void SchemaOptionConstraintsAreEnforced(string option, object value)
    {
        var options = new Dictionary<string, object?> { [option] = value };
        Assert.Throws<ArgumentException>(() => new EuchreGameService("g1", TestHelpers.PLAYERS, seed: TestSeeds.FromInt32(1), options));
    }

    private static int Seat(string playerId) => playerId switch {
        "p1" => TableSeats.North,
        "p2" => TableSeats.East,
        "p3" => TableSeats.South,
        "p4" => TableSeats.West,
        _ => throw new ArgumentOutOfRangeException(nameof(playerId))
    };
}
