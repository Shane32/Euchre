
namespace GameService.Tests;

public sealed class PinochleRuleTests
{
    [Fact]
    public void PinochleRanksAceTenKingQueenJackNine()
    {
        var cards = new[] { CardRank.Nine, CardRank.Jack, CardRank.Queen, CardRank.King, CardRank.Ten, CardRank.Ace }
            .Select(rank => TestHelpers.Card(rank, CardSuit.Clubs)).ToArray();
        var strengths = cards.Select(card => PinochleGameService.GetCardStrength(card, CardSuit.Clubs, CardSuit.Hearts)).ToArray();
        Assert.Equal(strengths.OrderBy(value => value), strengths);
    }

    [Fact]
    public void PlayerMustFollowSuit()
    {
        var clubAce = TestHelpers.Card(CardRank.Ace, CardSuit.Clubs, "ca");
        var heartAce = TestHelpers.Card(CardRank.Ace, CardSuit.Hearts, "ha");
        var legal = PinochleGameService.GetLegalCards([clubAce, heartAce], [TestHelpers.Card(CardRank.Nine, CardSuit.Clubs)], CardSuit.Hearts);
        Assert.Equal([clubAce], legal);
    }

    [Fact]
    public void PlayerMustBeatLedSuitWhenAble()
    {
        var clubAce = TestHelpers.Card(CardRank.Ace, CardSuit.Clubs, "ca");
        var clubNine = TestHelpers.Card(CardRank.Nine, CardSuit.Clubs, "c9");
        var legal = PinochleGameService.GetLegalCards([clubAce, clubNine], [TestHelpers.Card(CardRank.King, CardSuit.Clubs)], CardSuit.Hearts);
        Assert.Equal([clubAce], legal);
    }

    [Fact]
    public void PlayerMustTrumpWhenVoidInLedSuit()
    {
        var heartNine = TestHelpers.Card(CardRank.Nine, CardSuit.Hearts, "h9");
        var spadeAce = TestHelpers.Card(CardRank.Ace, CardSuit.Spades, "sa");
        var legal = PinochleGameService.GetLegalCards([heartNine, spadeAce], [TestHelpers.Card(CardRank.Ace, CardSuit.Clubs)], CardSuit.Hearts);
        Assert.Equal([heartNine], legal);
    }

    [Fact]
    public void PlayerMustOvertrumpWhenAble()
    {
        var heartAce = TestHelpers.Card(CardRank.Ace, CardSuit.Hearts, "ha");
        var heartNine = TestHelpers.Card(CardRank.Nine, CardSuit.Hearts, "h9");
        var trick = new[] { TestHelpers.Card(CardRank.Ace, CardSuit.Clubs), TestHelpers.Card(CardRank.King, CardSuit.Hearts) };
        var legal = PinochleGameService.GetLegalCards([heartAce, heartNine], trick, CardSuit.Hearts);
        Assert.Equal([heartAce], legal);
    }

    [Fact]
    public void PlayerMayDiscardWhenVoidAndWithoutTrump()
    {
        var cards = new[] { TestHelpers.Card(CardRank.Nine, CardSuit.Diamonds), TestHelpers.Card(CardRank.Ace, CardSuit.Spades) };
        Assert.Equal(cards, PinochleGameService.GetLegalCards(cards, [TestHelpers.Card(CardRank.Ace, CardSuit.Clubs)], CardSuit.Hearts));
    }

    [Fact]
    public void SingleRunScoresFifteenWithoutDoubleCountingItsMarriage()
    {
        var hand = new[] { CardRank.Ace, CardRank.Ten, CardRank.King, CardRank.Queen, CardRank.Jack }.Select(rank => TestHelpers.Card(rank, CardSuit.Hearts)).ToArray();
        var meld = PinochleGameService.CalculateMeld(hand, CardSuit.Hearts);
        Assert.Equal(15, meld.Total);
        Assert.Equal(15, meld.Values[PinochleMeld.Run]);
        Assert.DoesNotContain(PinochleMeld.RoyalMarriage, meld.Values.Keys);
    }

    [Fact]
    public void DoubleRunScoresOneHundredFifty()
    {
        var hand = Enumerable.Range(0, 2)
            .SelectMany(_ => new[] { CardRank.Ace, CardRank.Ten, CardRank.King, CardRank.Queen, CardRank.Jack })
            .Select(rank => TestHelpers.Card(rank, CardSuit.Spades)).ToArray();
        Assert.Equal(150, PinochleGameService.CalculateMeld(hand, CardSuit.Spades).Total);
    }

    [Fact]
    public void PlainAndRoyalMarriagesScoreCorrectly()
    {
        var hand = new[]
        {
            TestHelpers.Card(CardRank.King, CardSuit.Hearts), TestHelpers.Card(CardRank.Queen, CardSuit.Hearts),
            TestHelpers.Card(CardRank.King, CardSuit.Clubs), TestHelpers.Card(CardRank.Queen, CardSuit.Clubs)
        };
        var meld = PinochleGameService.CalculateMeld(hand, CardSuit.Hearts);
        Assert.Equal(6, meld.Total);
        Assert.Equal(4, meld.Values[PinochleMeld.RoyalMarriage]);
        Assert.Equal(2, meld.Values[PinochleMeld.Marriage]);
    }

    [Fact]
    public void DoubleRoyalMarriageScoresTwelveWithBonusVariation()
    {
        var hand = Enumerable.Range(0, 2)
            .SelectMany(_ => new[] { CardRank.King, CardRank.Queen })
            .Select(rank => TestHelpers.Card(rank, CardSuit.Hearts))
            .ToArray();

        var meld = PinochleGameService.CalculateMeld(
            hand,
            CardSuit.Hearts,
            PinochleMarriageScoring.DoubleMarriageBonus);

        Assert.Equal(12, meld.Total);
        Assert.Equal(12, meld.Values[PinochleMeld.DoubleRoyalMarriage]);
        Assert.DoesNotContain(PinochleMeld.RoyalMarriage, meld.Values.Keys);
    }

    [Fact]
    public void DoubleRoyalMarriageWithSingleRunScoresEightBeyondRunWithBonusVariation()
    {
        var hand = new[]
        {
            TestHelpers.Card(CardRank.Ace, CardSuit.Hearts),
            TestHelpers.Card(CardRank.Ten, CardSuit.Hearts),
            TestHelpers.Card(CardRank.Jack, CardSuit.Hearts),
            TestHelpers.Card(CardRank.King, CardSuit.Hearts),
            TestHelpers.Card(CardRank.King, CardSuit.Hearts),
            TestHelpers.Card(CardRank.Queen, CardSuit.Hearts),
            TestHelpers.Card(CardRank.Queen, CardSuit.Hearts)
        };

        var meld = PinochleGameService.CalculateMeld(
            hand,
            CardSuit.Hearts,
            PinochleMarriageScoring.DoubleMarriageBonus);

        Assert.Equal(23, meld.Total);
        Assert.Equal(15, meld.Values[PinochleMeld.Run]);
        Assert.Equal(8, meld.Values[PinochleMeld.DoubleRoyalMarriage]);
    }

    [Fact]
    public void DoublePlainMarriageScoresSixWithBonusVariation()
    {
        var hand = Enumerable.Range(0, 2)
            .SelectMany(_ => new[] { CardRank.King, CardRank.Queen })
            .Select(rank => TestHelpers.Card(rank, CardSuit.Clubs))
            .ToArray();

        var meld = PinochleGameService.CalculateMeld(
            hand,
            CardSuit.Hearts,
            PinochleMarriageScoring.DoubleMarriageBonus);

        Assert.Equal(6, meld.Total);
        Assert.Equal(6, meld.Values[PinochleMeld.DoubleMarriage]);
        Assert.DoesNotContain(PinochleMeld.Marriage, meld.Values.Keys);
    }

    [Fact]
    public void DoubleMarriagesKeepStandardScoresByDefault()
    {
        var hand = Enumerable.Range(0, 2)
            .SelectMany(_ => new[]
            {
                TestHelpers.Card(CardRank.King, CardSuit.Hearts),
                TestHelpers.Card(CardRank.Queen, CardSuit.Hearts),
                TestHelpers.Card(CardRank.King, CardSuit.Clubs),
                TestHelpers.Card(CardRank.Queen, CardSuit.Clubs)
            })
            .ToArray();

        var meld = PinochleGameService.CalculateMeld(hand, CardSuit.Hearts);

        Assert.Equal(12, meld.Total);
        Assert.Equal(8, meld.Values[PinochleMeld.RoyalMarriage]);
        Assert.Equal(4, meld.Values[PinochleMeld.Marriage]);
        Assert.DoesNotContain(PinochleMeld.DoubleRoyalMarriage, meld.Values.Keys);
        Assert.DoesNotContain(PinochleMeld.DoubleMarriage, meld.Values.Keys);
    }

    [Fact]
    public void MarriageScoringOptionAcceptsTypedBonusVariation()
    {
        var game = NewGame(new Dictionary<string, object?> {
            [PinochleOptionNames.MarriageScoring] = PinochleMarriageScoring.DoubleMarriageBonus
        });

        var value = game.GetGameState("p1").Options[PinochleOptionNames.MarriageScoring];

        Assert.Equal("doubleMarriageBonus", value.GetString());
    }

    [Fact]
    public void SingleAndDoublePinochleScoreCorrectly()
    {
        var single = new[] { TestHelpers.Card(CardRank.Queen, CardSuit.Spades), TestHelpers.Card(CardRank.Jack, CardSuit.Diamonds) };
        var doubled = single.Concat(new[] { TestHelpers.Card(CardRank.Queen, CardSuit.Spades), TestHelpers.Card(CardRank.Jack, CardSuit.Diamonds) }).ToArray();
        Assert.Equal(4, PinochleGameService.CalculateMeld(single, CardSuit.Clubs).Values[PinochleMeld.Pinochle]);
        Assert.Equal(30, PinochleGameService.CalculateMeld(doubled, CardSuit.Clubs).Values[PinochleMeld.DoublePinochle]);
    }

    [Theory]
    [InlineData(CardRank.Ace, 10)]
    [InlineData(CardRank.King, 8)]
    [InlineData(CardRank.Queen, 6)]
    [InlineData(CardRank.Jack, 4)]
    public void AroundsScoreForEachRank(CardRank rank, int expected)
    {
        var hand = Enum.GetValues<CardSuit>().Select(suit => TestHelpers.Card(rank, suit)).ToArray();
        Assert.Equal(expected, PinochleGameService.CalculateMeld(hand, CardSuit.Clubs).Total);
    }

    [Fact]
    public void DoubleAcesAroundScoresOneHundred()
    {
        var hand = Enumerable.Range(0, 2)
            .SelectMany(_ => Enum.GetValues<CardSuit>())
            .Select(suit => TestHelpers.Card(CardRank.Ace, suit)).ToArray();
        Assert.Equal(100, PinochleGameService.CalculateMeld(hand, CardSuit.Clubs).Total);
    }

    [Fact]
    public void EachNineOfTrumpScoresDix()
    {
        var hand = new[] { TestHelpers.Card(CardRank.Nine, CardSuit.Hearts), TestHelpers.Card(CardRank.Nine, CardSuit.Hearts) };
        Assert.Equal(2, PinochleGameService.CalculateMeld(hand, CardSuit.Hearts).Values[PinochleMeld.Dix]);
    }

    [Theory]
    [InlineData(20, 10, 10, 3, 5, true, 20, 8)]
    [InlineData(25, 10, 10, 3, 5, true, -25, 8)]
    [InlineData(25, 10, 10, 3, 5, false, -25, 0)]
    public void PinochleScoringMakesOrSetsTheBid(
        int bid, int bidderMeld, int bidderCards, int defenderMeld, int defenderCards,
        bool defendersTookTrick, int biddingDelta, int defendingDelta)
    {
        Assert.Equal(
            (biddingDelta, defendingDelta),
            PinochleGameService.ScoreDeal(bid, bidderMeld, bidderCards, defenderMeld, defenderCards, defendersTookTrick));
    }

    [Fact]
    public void PinochleDealsTwelveCardsToEveryPlayer()
    {
        var game = NewGame();
        game.StartGame();
        var state = game.GetGameState("p1");
        Assert.All(state.Players, player => Assert.Equal(12, TestHelpers.Hand(state, player.Seat).Cards.Count));
        Assert.Empty(TestHelpers.Pile(state, PileIds.Deck).Cards);
        Assert.Equal(TableSeats.East, Assert.Single(state.Turn.ActiveSeats));
    }

    [Fact]
    public void OpeningBidMustMeetMinimum()
    {
        var game = NewGame();
        game.StartGame();
        var error = Assert.Throws<GameRuleException>(() => game.Bid("p2", 19, "low"));
        Assert.Equal(GameRuleError.BidTooLow, error.Code);
    }

    [Fact]
    public void LaterBidMustExceedHighBid()
    {
        var game = NewGame();
        game.StartGame();
        game.Bid("p2", 20, "bid20");
        var error = Assert.Throws<GameRuleException>(() => game.Bid("p3", 20, "tie"));
        Assert.Equal(GameRuleError.BidTooLow, error.Code);
    }

    [Fact]
    public void ThreeOpeningPassesForceDealerToMinimumBid()
    {
        var game = NewGame();
        game.StartGame();
        game.Pass("p2", "pass1");
        game.Pass("p3", "pass2");
        game.Pass("p4", "pass3");

        var state = game.GetGameState("p1");
        Assert.Equal(GamePhase.ChooseTrump, state.Phase.Id);
        Assert.Equal(TableSeats.North, state.Contract!.DeclarerSeat);
        Assert.Equal(20, state.Contract.Value);
    }

    [Fact]
    public void HighBidderWinsAfterOtherPlayersPass()
    {
        var game = NewGame();
        game.StartGame();
        game.Bid("p2", 20, "bid");
        game.Pass("p3", "pass1");
        game.Pass("p4", "pass2");
        game.Pass("p1", "pass3");

        var state = game.GetGameState("p2");
        Assert.Equal(TableSeats.East, state.Contract!.DeclarerSeat);
        Assert.Equal(GamePhase.ChooseTrump, state.Phase.Id);
    }

    [Fact]
    public void TrumpMarriageIsRequiredWhenOptionEnabled()
    {
        var game = NewGame();
        game.StartGame();
        game.Bid("p2", 20, "bid");
        game.Pass("p3", "pass1");
        game.Pass("p4", "pass2");
        game.Pass("p1", "pass3");
        var hand = TestHelpers.Hand(game.GetGameState("p2"), TableSeats.East).Cards;
        var invalidSuit = Enum.GetValues<CardSuit>()
            .First(suit => !(hand.Any(card => card.Rank == CardRank.King && card.Suit == suit)
                && hand.Any(card => card.Rank == CardRank.Queen && card.Suit == suit)));

        var error = Assert.Throws<GameRuleException>(() => game.ChooseTrump("p2", invalidSuit, "trump"));
        Assert.Equal(GameRuleError.TrumpMarriageRequired, error.Code);
    }

    [Fact]
    public void CompletePinochleHandScoresAndAdvancesDeal()
    {
        var game = NewGame(new Dictionary<string, object?> {
            [PinochleOptionNames.RequireTrumpMarriage] = false,
            [PinochleOptionNames.TargetScore] = 1000
        });
        game.StartGame();
        game.Bid("p2", 20, "bid");
        game.Pass("p3", "pass1");
        game.Pass("p4", "pass2");
        game.Pass("p1", "pass3");
        game.ChooseTrump("p2", CardSuit.Hearts, "trump");

        var plays = 0;
        while (game.GetGameState("p1").DealNumber == 1) {
            var state = game.GetGameState("p1");
            var activeSeat = Assert.Single(state.Turn.ActiveSeats);
            var playerId = Player(activeSeat);
            var playerState = game.GetGameState(playerId);
            var hand = TestHelpers.Hand(playerState, activeSeat).Cards;
            var trick = TestHelpers.Pile(playerState, PileIds.CurrentTrick).Cards;
            var legal = PinochleGameService.GetLegalCards(hand, trick, playerState.Contract!.Suit!.Value);
            var index = hand.FindIndex(card => card.Ref == legal[0].Ref);
            game.Play(playerId, index, $"play-{++plays}");
            Assert.True(plays <= 48);
        }

        var final = game.GetGameState("p1");
        Assert.Equal(48, plays);
        Assert.Equal(2, final.DealNumber);
        Assert.Equal(GamePhase.Bidding, final.Phase.Id);
    }

    private static PinochleGameService NewGame(IReadOnlyDictionary<string, object?>? options = null)
        => new("pinochle-test", TestHelpers.PLAYERS, seed: TestSeeds.FromInt32(54321), options);

    private static string Player(int seat) => seat switch {
        TableSeats.North => "p1",
        TableSeats.East => "p2",
        TableSeats.South => "p3",
        TableSeats.West => "p4",
        _ => throw new ArgumentOutOfRangeException(nameof(seat))
    };
}
