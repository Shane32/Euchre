using System.Text.Json;

namespace GameService.Games.Pinochle;

/// <summary>Provides the game service implementation for Pinochle.</summary>
public sealed class PinochleGameService : GameServiceBase
{
    private static readonly CardSuit[] s_suits = Enum.GetValues<CardSuit>();
    private static readonly Dictionary<CardRank, int> s_ranks = new() {
        [CardRank.Nine] = 1,
        [CardRank.Jack] = 2,
        [CardRank.Queen] = 3,
        [CardRank.King] = 4,
        [CardRank.Ten] = 5,
        [CardRank.Ace] = 6
    };

    /// <summary>Initializes a new instance of the pinochle game service type.</summary>
    public PinochleGameService(
        string gameId,
        IReadOnlyList<PlayerRegistration> players,
        Guid? seed = null,
        IReadOnlyDictionary<string, object?>? options = null)
        : base(GameDefinitionLoader.LoadEmbedded(GameType.Pinochle), gameId, players, seed, options)
    {
        State.Scores.Target = GetOption<int>(PinochleOptionNames.TargetScore);
        State.DealerSeat = Definition.PlayerConfiguration.Seats[0];
    }

    /// <summary>Restores a game service from recorded history.</summary>
    public static PinochleGameService Restore(GameHistory history)
    {
        ArgumentNullException.ThrowIfNull(history);
        if (history.GameType != GameType.Pinochle) {
            throw new ArgumentException("The history is not for a Pinochle game.", nameof(history));
        }

        var game = new PinochleGameService(
            history.GameId,
            history.Players,
            history.BaseRandomSeed,
            GetReplayOptions(history));
        game.ReplayHistory(history);
        return game;
    }

    /// <summary>Starts the game.</summary>
    public override void StartGame()
    {
        ExecuteSystemAction(GameActionType.Deal, SystemActionIds.StartGame, () => {
            if (State.Status != GameStatus.Waiting) {
                throw Rule(GameRuleError.InvalidPhase, "The game has already started.");
            }
            State.Status = GameStatus.Active;
            State.RoundNumber = 1;
            State.DealNumber = 1;
            DealHand();
        });
    }

    /// <summary>Attempts to place a bid.</summary>
    public ActionResult Bid(string playerId, int value, string actionId, long? expectedRevision = null)
        => PerformAction(Request(playerId, GameActionType.Bid, new { value }, actionId, expectedRevision));

    /// <summary>Attempts to pass.</summary>
    public ActionResult Pass(string playerId, string actionId, long? expectedRevision = null)
        => PerformAction(Request(playerId, GameActionType.Pass, new { }, actionId, expectedRevision));

    /// <summary>Attempts to choose the trump contract.</summary>
    public ActionResult ChooseTrump(string playerId, CardSuit suit, string actionId, long? expectedRevision = null)
        => PerformAction(Request(playerId, GameActionType.ChooseTrump, new { suit }, actionId, expectedRevision));

    /// <summary>Attempts to play a card.</summary>
    public ActionResult Play(string playerId, int cardIndex, string actionId, long? expectedRevision = null)
        => PlayCard(new PlayCardRequest(playerId, PileIds.Hand(GetPlayer(playerId).Seat), cardIndex, PileIds.CurrentTrick, actionId, expectedRevision));

    /// <summary>Gets a card's relative trick-taking strength.</summary>
    public static int GetCardStrength(CardState card, CardSuit ledSuit, CardSuit trump)
    {
        var rank = s_ranks[card.Rank!.Value];
        if (card.Suit == trump) {
            return 200 + rank;
        }
        if (card.Suit == ledSuit) {
            return 100 + rank;
        }
        return rank;
    }

    /// <summary>Gets the cards that may be legally played.</summary>
    public static IReadOnlyList<CardState> GetLegalCards(
        IReadOnlyList<CardState> hand,
        IReadOnlyList<CardState> trick,
        CardSuit trump)
    {
        if (trick.Count == 0) {
            return hand.ToArray();
        }

        var ledSuit = trick[0].Suit!.Value;
        var winner = trick.Select((card, index) => (card, index))
            .OrderByDescending(item => GetCardStrength(item.card, ledSuit, trump))
            .ThenBy(item => item.index)
            .First().card;
        var following = hand.Where(card => card.Suit == ledSuit).ToArray();
        if (following.Length > 0) {
            if (winner.Suit == ledSuit) {
                var beating = following.Where(card => s_ranks[card.Rank!.Value] > s_ranks[winner.Rank!.Value]).ToArray();
                if (beating.Length > 0) {
                    return beating;
                }
            }
            return following;
        }

        var trumps = hand.Where(card => card.Suit == trump).ToArray();
        if (trumps.Length > 0) {
            if (winner.Suit == trump) {
                var overtrumps = trumps.Where(card => s_ranks[card.Rank!.Value] > s_ranks[winner.Rank!.Value]).ToArray();
                if (overtrumps.Length > 0) {
                    return overtrumps;
                }
            }
            return trumps;
        }

        return hand.ToArray();
    }

    /// <summary>Calculates the meld in a hand.</summary>
    public static PinochleMeldResult CalculateMeld(
        IReadOnlyCollection<CardState> hand,
        CardSuit trump,
        PinochleMarriageScoring marriageScoring = PinochleMarriageScoring.Standard)
    {
        int Count(CardRank rank, CardSuit suit) => hand.Count(card => card.Rank == rank && card.Suit == suit);
        var values = new Dictionary<PinochleMeld, int>();

        var runCount = new[] { CardRank.Ace, CardRank.Ten, CardRank.King, CardRank.Queen, CardRank.Jack }.Min(rank => Count(rank, trump));
        var runCardsUsed = 0;
        if (runCount >= 2) {
            values[PinochleMeld.DoubleRun] = 150;
            runCardsUsed = 2;
        } else if (runCount == 1) {
            values[PinochleMeld.Run] = 15;
            runCardsUsed = 1;
        }

        AddRoyalMarriages(values, marriageScoring, runCount, runCardsUsed,
            Math.Min(Count(CardRank.King, trump), Count(CardRank.Queen, trump)));
        AddPlainMarriages(values, marriageScoring, trump, Count);

        AddAround(values, PinochleMeld.AcesAround, PinochleMeld.DoubleAcesAround, CardRank.Ace, 10, 100, Count);
        AddAround(values, PinochleMeld.KingsAround, PinochleMeld.DoubleKingsAround, CardRank.King, 8, 80, Count);
        AddAround(values, PinochleMeld.QueensAround, PinochleMeld.DoubleQueensAround, CardRank.Queen, 6, 60, Count);
        AddAround(values, PinochleMeld.JacksAround, PinochleMeld.DoubleJacksAround, CardRank.Jack, 4, 40, Count);

        var pinochles = Math.Min(Count(CardRank.Queen, CardSuit.Spades), Count(CardRank.Jack, CardSuit.Diamonds));
        if (pinochles >= 2) {
            values[PinochleMeld.DoublePinochle] = 30;
        } else if (pinochles == 1) {
            values[PinochleMeld.Pinochle] = 4;
        }

        var dix = Count(CardRank.Nine, trump);
        if (dix > 0) {
            values[PinochleMeld.Dix] = dix;
        }

        return new PinochleMeldResult(values.Values.Sum(), values);
    }

    /// <summary>Scores the current deal.</summary>
    public static (int BiddingDelta, int DefendingDelta) ScoreDeal(
        int bid,
        int biddingMeld,
        int biddingCardPoints,
        int defendingMeld,
        int defendingCardPoints,
        bool defendersTookTrick)
    {
        var biddingTotal = biddingMeld + biddingCardPoints;
        var biddingDelta = biddingTotal >= bid ? biddingTotal : -bid;
        var defendingDelta = defendersTookTrick ? defendingMeld + defendingCardPoints : 0;
        return (biddingDelta, defendingDelta);
    }

    /// <summary>Applies a non-card game action.</summary>
    protected override void ApplyOtherAction(PlayerState player, GameActionType name, JsonElement arguments)
    {
        switch (name) {
            case GameActionType.Bid:
                ApplyBid(player, GetInteger(arguments, ActionArgumentNames.Value));
                break;
            case GameActionType.Pass:
                ApplyPass(player);
                break;
            case GameActionType.ChooseTrump:
                ApplyChooseTrump(player, GetEnum<CardSuit>(arguments, ActionArgumentNames.Suit));
                break;
            default:
                base.ApplyOtherAction(player, name, arguments);
                break;
        }
    }

    /// <summary>Validates a requested card play.</summary>
    protected override void ValidatePlayCard(PlayerState player, PileState source, CardState card, PileState target)
    {
        base.ValidatePlayCard(player, source, card, target);
        if (source.PileId != PileIds.Hand(player.Seat) || target.PileId != PileIds.CurrentTrick) {
            throw Rule(GameRuleError.InvalidPlay, "Cards must be played from the player's hand to the current trick.");
        }

        var trick = State.Tricks!.Plays.Select(play => GetCard(play.CardInstanceId)).ToList();
        var legal = GetLegalCards(source.Cards, trick, State.Contract!.Suit!.Value);
        if (!legal.Any(candidate => candidate.InstanceId == card.InstanceId)) {
            throw Rule(GameRuleError.IllegalPlay, "The player must follow suit, beat the trick, or trump when able.");
        }
    }

    /// <summary>Applies a validated card play.</summary>
    protected override void ApplyPlayCard(PlayerState player, PileState source, CardState card, PileState target)
    {
        MoveCard(source, card, target, faceUp: true);
        var tricks = State.Tricks!;
        tricks.Plays.Add(new TrickPlayState {
            Seat = player.Seat,
            CardInstanceId = card.InstanceId!,
            Order = tricks.Plays.Count
        });
        tricks.LedSuit ??= card.Suit;
        if (tricks.Plays.Count < 4) {
            SetActiveSeat(NextSeat(player.Seat));
            return;
        }
        CompleteTrick();
    }

    private void DealHand()
    {
        InitializeRandomForDeal();
        CollectAllCards(PileIds.Deck);
        ShufflePile(PileIds.Deck);
        ResetDealScores();
        State.Contract = null;
        State.Melds.Clear();
        State.Tricks = CreateTricksState();
        SetVariable<CardSuit?>(PinochleVariableNames.Trump, null);
        SetVariable<int?>(PinochleVariableNames.BidderSeat, null);
        SetVariable(PinochleVariableNames.TricksPlayed, 0);
        DealRoundRobin(PileIds.Deck, 12);

        var firstBidder = NextSeat(State.DealerSeat!.Value);
        State.Auction = new AuctionState { Round = 1, ActiveSeat = firstBidder };
        SetPhase(GamePhase.Bidding, TurnMode.ExclusiveTurn, firstBidder);
    }

    private void ApplyBid(PlayerState player, int value)
    {
        var auction = State.Auction!;
        if (auction.PassedSeats.Contains(player.Seat)) {
            throw Rule(GameRuleError.AlreadyPassed, "A player who passed may not re-enter bidding.");
        }
        var minimum = auction.HighCall?.Value + 1 ?? GetOption<int>(PinochleOptionNames.MinimumBid);
        if (value < minimum) {
            throw Rule(GameRuleError.BidTooLow, $"The bid must be at least {minimum}.");
        }
        var bid = new BidState { Seat = player.Seat, Kind = BidKind.Bid, Value = value, Revision = State.Revision + 1 };
        auction.Calls.Add(bid);
        auction.HighCall = bid;
        AdvanceAuction(player.Seat);
    }

    private void ApplyPass(PlayerState player)
    {
        var auction = State.Auction!;
        if (!auction.PassedSeats.AddIfMissing(player.Seat)) {
            throw Rule(GameRuleError.AlreadyPassed, "The player has already passed.");
        }
        auction.Calls.Add(new BidState { Seat = player.Seat, Kind = BidKind.Pass, Revision = State.Revision + 1 });

        if (auction.HighCall is null && auction.PassedSeats.Count == 3) {
            var forcedSeat = Definition.PlayerConfiguration.Seats.Single(seat => !auction.PassedSeats.Contains(seat));
            var forced = new BidState {
                Seat = forcedSeat,
                Kind = BidKind.Bid,
                Value = GetOption<int>(PinochleOptionNames.MinimumBid),
                Revision = State.Revision + 1
            };
            auction.Calls.Add(forced);
            auction.HighCall = forced;
            FinishAuction();
            return;
        }
        AdvanceAuction(player.Seat);
    }

    private void AdvanceAuction(int currentSeat)
    {
        var auction = State.Auction!;
        if (auction.HighCall is not null
            && Definition.PlayerConfiguration.Seats
                .Where(seat => seat != auction.HighCall.Seat)
                .All(seat => auction.PassedSeats.Contains(seat))) {
            FinishAuction();
            return;
        }

        var next = NextSeat(currentSeat);
        while (auction.PassedSeats.Contains(next)) {
            next = NextSeat(next);
        }
        auction.ActiveSeat = next;
        SetActiveSeat(next);
    }

    private void FinishAuction()
    {
        var auction = State.Auction!;
        var winningBid = auction.HighCall ?? throw new InvalidOperationException("Cannot finish an auction without a bid.");
        auction.Complete = true;
        auction.ActiveSeat = null;
        State.Contract = new ContractState {
            Kind = ContractKind.Pinochle,
            DeclarerSeat = winningBid.Seat,
            TeamId = GetTeamId(winningBid.Seat),
            Value = winningBid.Value
        };
        SetVariable(PinochleVariableNames.BidderSeat, winningBid.Seat);
        SetPhase(GamePhase.ChooseTrump, TurnMode.ExclusiveTurn, winningBid.Seat);
    }

    private void ApplyChooseTrump(PlayerState player, CardSuit suit)
    {
        var hand = GetPile(PileIds.Hand(player.Seat)).Cards;
        if (GetOption<bool>(PinochleOptionNames.RequireTrumpMarriage)
            && !(hand.Any(card => card.Rank == CardRank.King && card.Suit == suit)
                && hand.Any(card => card.Rank == CardRank.Queen && card.Suit == suit))) {
            throw Rule(GameRuleError.TrumpMarriageRequired, "The declarer must hold a king and queen in the chosen trump suit.");
        }

        State.Contract!.Suit = suit;
        State.Contract.TrumpKind = TrumpKind.Suit;
        SetVariable(PinochleVariableNames.Trump, suit);
        State.Melds.Clear();
        foreach (var participant in State.Players) {
            var meld = CalculateMeld(
                GetPile(PileIds.Hand(participant.Seat)).Cards,
                suit,
                GetOption<PinochleMarriageScoring>(PinochleOptionNames.MarriageScoring));
            foreach (var value in meld.Values) {
                State.Melds.Add(new PinochleMeldState {
                    Seat = participant.Seat,
                    TeamId = participant.TeamId,
                    Kind = value.Key,
                    Value = value.Value,
                    Revealed = true
                });
            }
            State.Scores.Teams[participant.TeamId!.Value].Deal += meld.Total;
        }

        State.Tricks = CreateTricksState();
        State.Tricks.LeaderSeat = player.Seat;
        State.Turn.LeadSeat = player.Seat;
        SetPhase(GamePhase.Play, TurnMode.ExclusiveTurn, player.Seat);
    }

    private void CompleteTrick()
    {
        var tricks = State.Tricks!;
        var trump = State.Contract!.Suit!.Value;
        var winningPlay = tricks.Plays
            .OrderByDescending(play => GetCardStrength(GetCard(play.CardInstanceId), tricks.LedSuit!.Value, trump))
            .ThenBy(play => play.Order)
            .First();
        var winnerTeam = GetTeamId(winningPlay.Seat)!.Value;
        var cards = tricks.Plays.Select(play => GetCard(play.CardInstanceId)).ToArray();
        var cardPoints = cards.Count(card => card.Rank is CardRank.Ace or CardRank.Ten or CardRank.King);

        tricks.WonBySeat[winningPlay.Seat]++;
        tricks.WonByTeam[winnerTeam]++;
        var score = State.Scores.Teams[winnerTeam];
        score.Counters[ScoreCounter.CardPoints] = score.Counters.GetValueOrDefault(ScoreCounter.CardPoints) + cardPoints;
        tricks.Completed.Add(new CompletedTrickState {
            Number = tricks.Completed.Count,
            LeaderSeat = tricks.LeaderSeat!.Value,
            WinnerSeat = winningPlay.Seat,
            WinningCardInstanceId = winningPlay.CardInstanceId,
            Plays = tricks.Plays.Select(play => new TrickPlayState {
                Seat = play.Seat,
                CardInstanceId = play.CardInstanceId,
                Order = play.Order
            }).ToList()
        });

        var current = GetPile(PileIds.CurrentTrick);
        var won = GetPile(PileIds.Won(winnerTeam));
        foreach (var card in current.Cards.ToArray()) {
            MoveCard(current, card, won, faceUp: true);
        }
        SetVariable(PinochleVariableNames.TricksPlayed, tricks.Completed.Count);

        if (tricks.Completed.Count == 12) {
            score.Counters[ScoreCounter.CardPoints]++;
            ScoreCurrentDeal();
            return;
        }

        tricks.Plays.Clear();
        tricks.LedSuit = null;
        tricks.LeaderSeat = winningPlay.Seat;
        State.Turn.LeadSeat = winningPlay.Seat;
        SetPhase(GamePhase.Play, TurnMode.ExclusiveTurn, winningPlay.Seat);
    }

    private void ScoreCurrentDeal()
    {
        SetPhase(GamePhase.ScoreDeal, TurnMode.Automatic);
        var biddingTeam = State.Contract!.TeamId!.Value;
        var defendingTeam = State.Teams.Single(team => team.TeamId != biddingTeam).TeamId;
        var biddingScore = State.Scores.Teams[biddingTeam];
        var defendingScore = State.Scores.Teams[defendingTeam];
        var result = ScoreDeal(
            State.Contract.Value!.Value,
            biddingScore.Deal,
            biddingScore.Counters.GetValueOrDefault(ScoreCounter.CardPoints),
            defendingScore.Deal,
            defendingScore.Counters.GetValueOrDefault(ScoreCounter.CardPoints),
            State.Tricks!.WonByTeam[defendingTeam] > 0);
        biddingScore.Deal = result.BiddingDelta;
        defendingScore.Deal = result.DefendingDelta;
        biddingScore.Total += result.BiddingDelta;
        defendingScore.Total += result.DefendingDelta;

        var candidates = State.Scores.Teams
            .Where(pair => pair.Value.Total >= State.Scores.Target)
            .OrderByDescending(pair => pair.Key == biddingTeam)
            .ThenByDescending(pair => pair.Value.Total)
            .ToArray();
        if (candidates.Length > 0) {
            State.Status = GameStatus.Completed;
            State.Result = new GameResultState {
                WinnerTeamIds = [candidates[0].Key],
                Reason = GameResultReason.TargetScore,
                CompletedAt = ActionTimestamp
            };
            SetPhase(GamePhase.Completed, TurnMode.Automatic);
            return;
        }

        State.DealerSeat = NextSeat(State.DealerSeat!.Value);
        State.RoundNumber++;
        State.DealNumber++;
        DealHand();
    }

    private TricksState CreateTricksState()
    {
        var tricks = new TricksState { CurrentPileId = PileIds.CurrentTrick };
        foreach (var seat in Definition.PlayerConfiguration.Seats) {
            tricks.WonBySeat[seat] = 0;
        }
        foreach (var team in State.Teams) {
            tricks.WonByTeam[team.TeamId] = 0;
        }
        return tricks;
    }

    private void ResetDealScores()
    {
        foreach (var score in State.Scores.Teams.Values) {
            score.Deal = 0;
            score.Counters[ScoreCounter.CardPoints] = 0;
        }
    }

    private void SetActiveSeat(int seat)
    {
        State.Turn.ActiveSeats = [seat];
        State.Turn.PrioritySeat = seat;
        if (State.Auction is not null && State.Phase.Id == GamePhase.Bidding) {
            State.Auction.ActiveSeat = seat;
        }
    }

    private static void AddAround(
        IDictionary<PinochleMeld, int> values,
        PinochleMeld singleName,
        PinochleMeld doubleName,
        CardRank rank,
        int singleValue,
        int doubleValue,
        Func<CardRank, CardSuit, int> count)
    {
        var copies = s_suits.Min(suit => count(rank, suit));
        if (copies >= 2) {
            values[doubleName] = doubleValue;
        } else if (copies == 1) {
            values[singleName] = singleValue;
        }
    }

    private static void AddRoyalMarriages(
        IDictionary<PinochleMeld, int> values,
        PinochleMarriageScoring marriageScoring,
        int runCount,
        int runCardsUsed,
        int royalMarriageCount)
    {
        if (marriageScoring == PinochleMarriageScoring.DoubleMarriageBonus
            && royalMarriageCount >= 2) {
            if (runCount == 0) {
                values[PinochleMeld.DoubleRoyalMarriage] = 12;
            } else if (runCount == 1) {
                values[PinochleMeld.DoubleRoyalMarriage] = 8;
            }
            return;
        }

        var extraRoyalMarriages = Math.Max(0, royalMarriageCount - runCardsUsed);
        if (extraRoyalMarriages > 0) {
            values[PinochleMeld.RoyalMarriage] = extraRoyalMarriages * 4;
        }
    }

    private static void AddPlainMarriages(
        IDictionary<PinochleMeld, int> values,
        PinochleMarriageScoring marriageScoring,
        CardSuit trump,
        Func<CardRank, CardSuit, int> count)
    {
        var singleValue = 0;
        var doubleValue = 0;
        foreach (var suit in s_suits) {
            if (suit == trump) {
                continue;
            }
            var marriageCount = Math.Min(count(CardRank.King, suit), count(CardRank.Queen, suit));
            if (marriageScoring == PinochleMarriageScoring.DoubleMarriageBonus
                && marriageCount >= 2) {
                doubleValue += 6;
            } else {
                singleValue += marriageCount * 2;
            }
        }
        if (singleValue > 0) {
            values[PinochleMeld.Marriage] = singleValue;
        }
        if (doubleValue > 0) {
            values[PinochleMeld.DoubleMarriage] = doubleValue;
        }
    }

    private static GameActionRequest Request(string playerId, GameActionType name, object arguments, string actionId, long? revision)
        => new(playerId, name, JsonSerializer.SerializeToElement(arguments), actionId, revision);

    private static int GetInteger(JsonElement arguments, string property)
        => arguments.TryGetProperty(property, out var value) && value.TryGetInt32(out var result)
            ? result
            : throw Rule(GameRuleError.InvalidArguments, $"Integer argument '{property}' is required.");

    private static T GetEnum<T>(JsonElement arguments, string property) where T : struct, Enum
        => arguments.TryGetProperty(property, out var value)
            && value.ValueKind == JsonValueKind.String
            && Enum.TryParse<T>(value.GetString(), ignoreCase: true, out var result)
                ? result
                : throw Rule(GameRuleError.InvalidArguments, $"Valid {typeof(T).Name} argument '{property}' is required.");
}
