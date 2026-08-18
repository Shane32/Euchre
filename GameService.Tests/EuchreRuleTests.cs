
namespace GameService.Tests;

public sealed class EuchreRuleTests
{
    [Theory]
    [InlineData(CardSuit.Hearts, CardSuit.Diamonds)]
    [InlineData(CardSuit.Diamonds, CardSuit.Hearts)]
    [InlineData(CardSuit.Clubs, CardSuit.Spades)]
    [InlineData(CardSuit.Spades, CardSuit.Clubs)]
    public void LeftBowerChangesToTrumpSuit(CardSuit trump, CardSuit printedSuit)
    {
        var left = TestHelpers.Card(CardRank.Jack, printedSuit);
        Assert.Equal(trump, EuchreGameService.GetEffectiveSuit(left, trump));
    }

    [Fact]
    public void RightThenLeftBowerAreHighestTrump()
    {
        var right = TestHelpers.Card(CardRank.Jack, CardSuit.Hearts);
        var left = TestHelpers.Card(CardRank.Jack, CardSuit.Diamonds);
        var ace = TestHelpers.Card(CardRank.Ace, CardSuit.Hearts);

        Assert.True(EuchreGameService.GetCardStrength(right, CardSuit.Clubs, CardSuit.Hearts)
            > EuchreGameService.GetCardStrength(left, CardSuit.Clubs, CardSuit.Hearts));
        Assert.True(EuchreGameService.GetCardStrength(left, CardSuit.Clubs, CardSuit.Hearts)
            > EuchreGameService.GetCardStrength(ace, CardSuit.Clubs, CardSuit.Hearts));
    }

    [Fact]
    public void LeftBowerDoesNotFollowItsPrintedSuit()
    {
        var left = TestHelpers.Card(CardRank.Jack, CardSuit.Diamonds, "left");
        var diamondAce = TestHelpers.Card(CardRank.Ace, CardSuit.Diamonds, "ace-d");
        var hand = new[] { left, diamondAce };
        var trick = new[] { TestHelpers.Card(CardRank.Nine, CardSuit.Diamonds) };

        Assert.False(EuchreGameService.CanPlayCard(hand, left, trick, CardSuit.Hearts));
        Assert.True(EuchreGameService.CanPlayCard(hand, diamondAce, trick, CardSuit.Hearts));
    }

    [Fact]
    public void LeftBowerMustBePlayedWhenTrumpIsLedAndItIsOnlyTrump()
    {
        var left = TestHelpers.Card(CardRank.Jack, CardSuit.Diamonds, "left");
        var clubAce = TestHelpers.Card(CardRank.Ace, CardSuit.Clubs, "ace-c");
        var hand = new[] { left, clubAce };
        var trick = new[] { TestHelpers.Card(CardRank.Nine, CardSuit.Hearts) };

        Assert.True(EuchreGameService.CanPlayCard(hand, left, trick, CardSuit.Hearts));
        Assert.False(EuchreGameService.CanPlayCard(hand, clubAce, trick, CardSuit.Hearts));
    }

    [Theory]
    [InlineData(0, false, 0, 2)]
    [InlineData(2, true, 0, 2)]
    [InlineData(3, false, 1, 0)]
    [InlineData(4, true, 1, 0)]
    [InlineData(5, false, 2, 0)]
    [InlineData(5, true, 4, 0)]
    public void EuchreScoringMatchesStandardRules(int tricks, bool alone, int maker, int defenders)
    {
        Assert.Equal((maker, defenders), EuchreGameService.ScoreDeal(tricks, alone));
    }

    [Fact]
    public void AceNoFaceRequiresExactlyOneAceAndNoFaceCards()
    {
        var valid = new[]
        {
            TestHelpers.Card(CardRank.Ace, CardSuit.Clubs), TestHelpers.Card(CardRank.Nine, CardSuit.Clubs),
            TestHelpers.Card(CardRank.Nine, CardSuit.Diamonds), TestHelpers.Card(CardRank.Ten, CardSuit.Hearts),
            TestHelpers.Card(CardRank.Ten, CardSuit.Spades)
        };
        var hasFace = valid.Select(card => TestHelpers.Card(card.Rank!.Value, card.Suit!.Value)).ToArray();
        hasFace[4] = TestHelpers.Card(CardRank.Jack, CardSuit.Spades);

        Assert.True(EuchreGameService.IsAceNoFace(valid));
        Assert.False(EuchreGameService.IsAceNoFace(hasFace));
    }

    [Fact]
    public void DealStartsBiddingLeftOfDealer()
    {
        var game = NewGame();
        game.StartGame();
        var state = game.GetGameState("p1");

        Assert.Equal(TableSeats.North, state.DealerSeat);
        Assert.Equal(GamePhase.BidRoundOne, state.Phase.Id);
        Assert.Equal([TableSeats.East], state.Turn.ActiveSeats);
        Assert.All(state.Players, player => Assert.Equal(5, TestHelpers.Hand(state, player.Seat).Cards.Count));
        Assert.Equal(4, TestHelpers.Pile(state, PileIds.Kitty).Cards.Count);
    }

    [Fact]
    public void FourFirstRoundPassesStartSecondRound()
    {
        var game = NewGame();
        game.StartGame();
        game.Pass("p2", "p1");
        game.Pass("p3", "p2");
        game.Pass("p4", "p3");
        game.Pass("p1", "p4");

        var state = game.GetGameState("p1");
        Assert.Equal(GamePhase.BidRoundTwo, state.Phase.Id);
        Assert.Equal([TableSeats.East], state.Turn.ActiveSeats);
    }

    [Fact]
    public void StickTheDealerPreventsDealersSecondRoundPass()
    {
        var game = NewGame(new Dictionary<string, object?> { [EuchreOptionNames.StickTheDealer] = true });
        game.StartGame();
        PassFirstRound(game);
        game.Pass("p2", "s1");
        game.Pass("p3", "s2");
        game.Pass("p4", "s3");

        Assert.DoesNotContain(GameActionType.Pass, game.GetLegalActions("p1"));
        var error = Assert.Throws<GameRuleException>(() => game.Pass("p1", "s4"));
        Assert.Equal(GameRuleError.DealerMustBid, error.Code);
    }

    [Fact]
    public void AnkleGivesOnlyFinalBidderSuitNoTrumpAndPassChoices()
    {
        var game = NewGame(new Dictionary<string, object?> { [EuchreOptionNames.Ankle] = true });
        game.StartGame();
        PassFirstRound(game);

        Assert.DoesNotContain(GameActionType.ChooseNoTrump, game.GetLegalActions("p2"));
        var earlyError = Assert.Throws<GameRuleException>(() => game.ChooseNoTrump("p2", false, "early"));
        Assert.Equal(GameRuleError.InvalidNoTrump, earlyError.Code);

        game.Pass("p2", "s1");
        game.Pass("p3", "s2");
        game.Pass("p4", "s3");

        Assert.Equal(
            [GameActionType.Pass, GameActionType.ChooseTrump, GameActionType.ChooseNoTrump],
            game.GetLegalActions("p1"));
    }

    [Fact]
    public void AnkleFinalBidderCanChooseNoTrump()
    {
        var game = NewGame(new Dictionary<string, object?> { [EuchreOptionNames.Ankle] = true });
        game.StartGame();
        PassToAnkleDecision(game);
        game.ChooseNoTrump("p1", false, "no-trump");

        var state = game.GetGameState("p1");
        Assert.Equal(GamePhase.Play, state.Phase.Id);
        Assert.Equal(TableSeats.North, state.Contract!.DeclarerSeat);
        Assert.Equal(TrumpKind.NoTrump, state.Contract.TrumpKind);
        Assert.Null(state.Contract.Suit);
        Assert.True(state.Variables[EuchreVariableNames.NoTrump].GetBoolean());
    }

    [Fact]
    public void GoingAloneAtNoTrumpSkipsFinalBiddersPartner()
    {
        var game = NewGame(new Dictionary<string, object?> { [EuchreOptionNames.Ankle] = true });
        game.StartGame();
        PassToAnkleDecision(game);
        game.ChooseNoTrump("p1", true, "no-trump-alone");

        Assert.Equal(TableSeats.East, Assert.Single(game.GetGameState("p2").Turn.ActiveSeats));
        game.Play("p2", 0, "play-east");
        Assert.Equal(TableSeats.West, Assert.Single(game.GetGameState("p4").Turn.ActiveSeats));
    }

    [Fact]
    public void AnkleNoTrumpUsesPrintedSuitAndOrdinaryJackRank()
    {
        var jackOfDiamonds = TestHelpers.Card(CardRank.Jack, CardSuit.Diamonds, "jd");
        var clubNine = TestHelpers.Card(CardRank.Nine, CardSuit.Clubs, "c9");
        var diamondLead = TestHelpers.Card(CardRank.Nine, CardSuit.Diamonds, "d9");
        var jackOfHearts = TestHelpers.Card(CardRank.Jack, CardSuit.Hearts, "jh");
        var aceOfHearts = TestHelpers.Card(CardRank.Ace, CardSuit.Hearts, "ah");

        Assert.Equal(CardSuit.Diamonds, EuchreGameService.GetEffectiveSuit(jackOfDiamonds, null));
        Assert.True(EuchreGameService.CanPlayCard([jackOfDiamonds, clubNine], jackOfDiamonds, [diamondLead], null));
        Assert.False(EuchreGameService.CanPlayCard([jackOfDiamonds, clubNine], clubNine, [diamondLead], null));
        Assert.True(EuchreGameService.GetCardStrength(aceOfHearts, CardSuit.Hearts, null)
            > EuchreGameService.GetCardStrength(jackOfHearts, CardSuit.Hearts, null));
    }

    [Fact]
    public void AnkleFinalBidderMayPassForRedeal()
    {
        var game = NewGame(new Dictionary<string, object?> { [EuchreOptionNames.Ankle] = true });
        game.StartGame();
        PassToAnkleDecision(game);
        game.Pass("p1", "final-pass");

        var state = game.GetGameState("p1");
        Assert.Equal(2, state.DealNumber);
        Assert.Equal(TableSeats.East, state.DealerSeat);
        Assert.Equal(GamePhase.BidRoundOne, state.Phase.Id);
    }

    [Fact]
    public void CompleteAnkleNoTrumpHandScoresAndAdvancesDeal()
    {
        var game = NewGame(new Dictionary<string, object?> {
            [EuchreOptionNames.Ankle] = true,
            [EuchreOptionNames.TargetScore] = 21
        });
        game.StartGame();
        PassToAnkleDecision(game);
        game.ChooseNoTrump("p1", false, "no-trump");

        var play = 0;
        while (game.GetGameState("p1").DealNumber == 1) {
            var state = game.GetGameState("p1");
            var activeSeat = Assert.Single(state.Turn.ActiveSeats);
            var playerId = Player(activeSeat);
            var playerState = game.GetGameState(playerId);
            var hand = TestHelpers.Hand(playerState, activeSeat).Cards;
            var trick = TestHelpers.Pile(playerState, PileIds.CurrentTrick).Cards;
            var legal = hand.Select((card, index) => (card, index))
                .First(item => EuchreGameService.CanPlayCard(hand, item.card, trick, null));
            game.Play(playerId, legal.index, $"ankle-play-{++play}");
            Assert.True(play <= 20);
        }

        Assert.Equal(20, play);
        Assert.Equal(2, game.GetGameState("p1").DealNumber);
    }

    [Fact]
    public void AnkleRemovesForcedBidWhenStickTheDealerIsAlsoEnabled()
    {
        var game = NewGame(new Dictionary<string, object?> {
            [EuchreOptionNames.Ankle] = true,
            [EuchreOptionNames.StickTheDealer] = true
        });
        game.StartGame();
        PassToAnkleDecision(game);

        Assert.Contains(GameActionType.Pass, game.GetLegalActions("p1"));
        game.Pass("p1", "ankle-overrides-stick");
        Assert.Equal(2, game.GetGameState("p1").DealNumber);
    }

    [Fact]
    public void AllPassRotatesDealerAndRedealsWhenDealerIsNotStuck()
    {
        var game = NewGame();
        game.StartGame();
        PassFirstRound(game);
        game.Pass("p2", "s1");
        game.Pass("p3", "s2");
        game.Pass("p4", "s3");
        game.Pass("p1", "s4");

        var state = game.GetGameState("p1");
        Assert.Equal(TableSeats.East, state.DealerSeat);
        Assert.Equal(2, state.DealNumber);
        Assert.Equal(GamePhase.BidRoundOne, state.Phase.Id);
    }

    [Fact]
    public void OrderUpMakesDealerPickUpThenDiscard()
    {
        var game = NewGame();
        game.StartGame();
        game.OrderUp("p2", alone: false, "order");

        var dealerState = game.GetGameState("p1");
        Assert.Equal(GamePhase.DealerDiscard, dealerState.Phase.Id);
        Assert.Equal(6, TestHelpers.Hand(dealerState, TableSeats.North).Cards.Count);

        game.Discard("p1", 0, "discard");
        dealerState = game.GetGameState("p1");
        Assert.Equal(GamePhase.Play, dealerState.Phase.Id);
        Assert.Equal(5, TestHelpers.Hand(dealerState, TableSeats.North).Cards.Count);
    }

    [Fact]
    public void GoingAloneSkipsMakersPartner()
    {
        var game = NewGame();
        game.StartGame();
        game.OrderUp("p2", alone: true, "order");
        game.Discard("p1", 0, "discard");

        var state = game.GetGameState("p2");
        Assert.Equal(TableSeats.West, state.Contract!.PartnerSeat);
        Assert.Equal(TableSeats.East, Assert.Single(state.Turn.ActiveSeats));
        game.Play("p2", 0, "play-east");
        Assert.Equal(TableSeats.South, Assert.Single(game.GetGameState("p3").Turn.ActiveSeats));
        game.Play("p3", 0, "play-south");
        Assert.Equal(TableSeats.North, Assert.Single(game.GetGameState("p1").Turn.ActiveSeats));
    }

    [Fact]
    public void SecondRoundCannotChooseTurnedSuit()
    {
        var game = NewGame();
        game.StartGame();
        var upcardSuit = TestHelpers.Pile(game.GetGameState("p2"), PileIds.Kitty).Cards.Single(card => !card.Unknown).Suit!.Value;
        PassFirstRound(game);

        var error = Assert.Throws<GameRuleException>(() => game.ChooseTrump("p2", upcardSuit, false, "bad-trump"));
        Assert.Equal(GameRuleError.InvalidTrump, error.Code);
    }

    [Fact]
    public void CompleteEuchreHandAdvancesToAnotherDeal()
    {
        var game = NewGame();
        game.StartGame();
        game.OrderUp("p2", false, "order");
        game.Discard("p1", 0, "discard");

        var play = 0;
        while (game.GetGameState("p1").DealNumber == 1) {
            var state = game.GetGameState("p1");
            var activeSeat = Assert.Single(state.Turn.ActiveSeats);
            var playerId = Player(activeSeat);
            var playerState = game.GetGameState(playerId);
            var hand = TestHelpers.Hand(playerState, activeSeat).Cards;
            var trick = TestHelpers.Pile(playerState, PileIds.CurrentTrick).Cards;
            var legal = hand.Select((card, index) => (card, index))
                .First(item => EuchreGameService.CanPlayCard(hand, item.card, trick, playerState.Contract!.Suit!.Value));
            game.Play(playerId, legal.index, $"play-{++play}");
            Assert.True(play <= 20);
        }

        var final = game.GetGameState("p1");
        Assert.Equal(2, final.DealNumber);
        Assert.True(final.Scores.Teams.Values.Sum(score => score.Total) > 0);
    }

    private static EuchreGameService NewGame(IReadOnlyDictionary<string, object?>? options = null)
        => new("euchre-test", TestHelpers.PLAYERS, seed: TestSeeds.FromInt32(12345), options);

    private static void PassFirstRound(EuchreGameService game)
    {
        game.Pass("p2", "f1");
        game.Pass("p3", "f2");
        game.Pass("p4", "f3");
        game.Pass("p1", "f4");
    }

    private static void PassToAnkleDecision(EuchreGameService game)
    {
        PassFirstRound(game);
        game.Pass("p2", "a1");
        game.Pass("p3", "a2");
        game.Pass("p4", "a3");
    }

    private static string Player(int seat) => seat switch {
        TableSeats.North => "p1",
        TableSeats.East => "p2",
        TableSeats.South => "p3",
        TableSeats.West => "p4",
        _ => throw new ArgumentOutOfRangeException(nameof(seat))
    };
}
