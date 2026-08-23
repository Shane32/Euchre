using System.Text.Json;

namespace GameService.Games.Euchre;

/// <summary>Provides the game service implementation for Euchre.</summary>
public sealed class EuchreGameService : GameServiceBase
{
    private static readonly Dictionary<CardRank, int> s_ranks = new() {
        [CardRank.Nine] = 1,
        [CardRank.Ten] = 2,
        [CardRank.Jack] = 3,
        [CardRank.Queen] = 4,
        [CardRank.King] = 5,
        [CardRank.Ace] = 6
    };

    /// <summary>Initializes a new instance of the euchre game service type.</summary>
    public EuchreGameService(
        string gameId,
        IReadOnlyList<PlayerRegistration> players,
        Guid? seed = null,
        IReadOnlyDictionary<string, object?>? options = null)
        : base(GameDefinitionLoader.LoadEmbedded(GameType.Euchre), gameId, players, seed, options)
    {
        State.Scores.Target = GetOption<int>(EuchreOptionNames.TargetScore);
        State.DealerSeat = Definition.PlayerConfiguration.Seats[0];
    }

    /// <summary>Restores a game service from recorded history.</summary>
    public static EuchreGameService Restore(GameHistory history)
    {
        ArgumentNullException.ThrowIfNull(history);
        if (history.GameType != GameType.Euchre) {
            throw new ArgumentException("The history is not for a Euchre game.", nameof(history));
        }

        var game = new EuchreGameService(
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

    /// <summary>Attempts to pass.</summary>
    public ActionResult Pass(string playerId, string actionId, long? expectedRevision = null)
        => PerformAction(Request(playerId, GameActionType.Pass, new { }, actionId, expectedRevision));

    /// <summary>Attempts to order up the turned suit.</summary>
    public ActionResult OrderUp(string playerId, bool alone, string actionId, long? expectedRevision = null)
        => PerformAction(Request(playerId, GameActionType.OrderUp, new { alone }, actionId, expectedRevision));

    /// <summary>Attempts to choose the trump contract.</summary>
    public ActionResult ChooseTrump(string playerId, CardSuit suit, bool alone, string actionId, long? expectedRevision = null)
        => PerformAction(Request(playerId, GameActionType.ChooseTrump, new { suit, alone }, actionId, expectedRevision));

    /// <summary>Attempts to choose a no-trump contract.</summary>
    public ActionResult ChooseNoTrump(string playerId, bool alone, string actionId, long? expectedRevision = null)
        => PerformAction(Request(playerId, GameActionType.ChooseNoTrump, new { alone }, actionId, expectedRevision));

    /// <summary>Attempts to claim an ace-no-face hand.</summary>
    public ActionResult ClaimAceNoFace(string playerId, string actionId, long? expectedRevision = null)
        => PerformAction(Request(playerId, GameActionType.ClaimAceNoFace, new { }, actionId, expectedRevision));

    /// <summary>Attempts to discard a card.</summary>
    public ActionResult Discard(string playerId, int cardIndex, string actionId, long? expectedRevision = null)
        => PlayCard(new PlayCardRequest(playerId, PileIds.Hand(GetSeat(playerId)), cardIndex, PileIds.Kitty, actionId, expectedRevision));

    /// <summary>Attempts to play a card.</summary>
    public ActionResult Play(string playerId, int cardIndex, string actionId, long? expectedRevision = null)
        => PlayCard(new PlayCardRequest(playerId, PileIds.Hand(GetSeat(playerId)), cardIndex, PileIds.CurrentTrick, actionId, expectedRevision));

    /// <summary>Gets a card's effective suit for the current contract.</summary>
    public static CardSuit GetEffectiveSuit(CardState card, CardSuit? trump)
    {
        if (trump.HasValue && card.Rank == CardRank.Jack && card.Suit == SameColorSuit(trump.Value)) {
            return trump.Value;
        }
        return card.Suit ?? throw new ArgumentException("Card suit is required.", nameof(card));
    }

    /// <summary>Gets a card's relative trick-taking strength.</summary>
    public static int GetCardStrength(CardState card, CardSuit ledSuit, CardSuit? trump)
    {
        var effectiveSuit = GetEffectiveSuit(card, trump);
        if (trump.HasValue && card.Rank == CardRank.Jack && card.Suit == trump.Value) {
            return 200;
        }
        if (trump.HasValue && card.Rank == CardRank.Jack && card.Suit == SameColorSuit(trump.Value)) {
            return 199;
        }
        if (trump.HasValue && effectiveSuit == trump.Value) {
            return 150 + s_ranks[card.Rank!.Value];
        }
        if (effectiveSuit == ledSuit) {
            return 100 + s_ranks[card.Rank!.Value];
        }
        return s_ranks[card.Rank!.Value];
    }

    /// <summary>Determines whether a card can be legally played.</summary>
    public static bool CanPlayCard(
        IReadOnlyCollection<CardState> hand,
        CardState selected,
        IReadOnlyList<CardState> trick,
        CardSuit? trump)
    {
        if (trick.Count == 0) {
            return true;
        }
        var ledSuit = GetEffectiveSuit(trick[0], trump);
        return GetEffectiveSuit(selected, trump) == ledSuit
            || !hand.Any(card => GetEffectiveSuit(card, trump) == ledSuit);
    }

    /// <summary>Scores the current deal.</summary>
    public static (int MakerPoints, int DefenderPoints) ScoreDeal(int makerTricks, bool alone)
    {
        if (makerTricks < 0 || makerTricks > 5) {
            throw new ArgumentOutOfRangeException(nameof(makerTricks));
        }
        if (makerTricks < 3) {
            return (0, 2);
        }
        if (makerTricks == 5) {
            return (alone ? 4 : 2, 0);
        }
        return (1, 0);
    }

    /// <summary>Determines whether a hand qualifies for an ace-no-face redeal.</summary>
    public static bool IsAceNoFace(IReadOnlyCollection<CardState> hand)
        => hand.Count == 5
            && hand.Count(card => card.Rank == CardRank.Ace) == 1
            && hand.All(card => card.Rank is CardRank.Ace or CardRank.Nine or CardRank.Ten);

    /// <summary>Determines whether an action is available to a player.</summary>
    protected override bool IsActionAvailable(PlayerState player, GameActionType actionId)
    {
        if (actionId == GameActionType.Pass && State.Phase.Id == GamePhase.BidRoundTwo
            && player.Seat == State.DealerSeat
            && GetOption<bool>(EuchreOptionNames.StickTheDealer)
            && !GetOption<bool>(EuchreOptionNames.Ankle)) {
            return false;
        }
        if (actionId == GameActionType.ClaimAceNoFace) {
            return GetOption<bool>(EuchreOptionNames.AceNoFaceRedeal) && IsAceNoFace(GetPile(PileIds.Hand(player.Seat)).Cards);
        }
        if (actionId == GameActionType.ChooseNoTrump) {
            return GetOption<bool>(EuchreOptionNames.Ankle)
                && State.Phase.Id == GamePhase.BidRoundTwo
                && player.Seat == State.DealerSeat;
        }
        return true;
    }

    /// <summary>Applies a non-card game action.</summary>
    protected override void ApplyOtherAction(PlayerState player, GameActionType name, JsonElement arguments)
    {
        switch (name) {
            case GameActionType.Pass:
                ApplyPass(player);
                break;
            case GameActionType.OrderUp:
                ApplyOrderUp(player, GetBoolean(arguments, ActionArgumentNames.Alone));
                break;
            case GameActionType.ChooseTrump:
                ApplyChooseTrump(player, GetEnum<CardSuit>(arguments, ActionArgumentNames.Suit), GetBoolean(arguments, ActionArgumentNames.Alone));
                break;
            case GameActionType.ChooseNoTrump:
                ApplyChooseNoTrump(player, GetBoolean(arguments, ActionArgumentNames.Alone));
                break;
            case GameActionType.ClaimAceNoFace:
                ApplyAceNoFace(player);
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
        if (State.Phase.Id == GamePhase.DealerDiscard) {
            if (player.Seat != State.DealerSeat || source.PileId != PileIds.Hand(player.Seat) || target.PileId != PileIds.Kitty) {
                throw Rule(GameRuleError.InvalidDiscard, "The dealer must discard one card from their hand to the kitty.");
            }
            if (source.Cards.Count != 6) {
                throw Rule(GameRuleError.InvalidDiscard, "The dealer must have six cards before discarding.");
            }
            return;
        }

        if (source.PileId != PileIds.Hand(player.Seat) || target.PileId != PileIds.CurrentTrick) {
            throw Rule(GameRuleError.InvalidPlay, "Cards must be played from the player's hand to the current trick.");
        }
        var contract = State.Contract ?? throw new InvalidOperationException("A contract has not been selected.");
        var trump = GetTrumpSuit(contract);
        var trickCards = State.Tricks!.Plays.Select(play => GetCard(play.CardInstanceId)).ToList();
        if (!CanPlayCard(source.Cards, card, trickCards, trump)) {
            throw Rule(GameRuleError.MustFollowSuit, "The player must follow the effective suit led.");
        }
    }

    /// <summary>Applies a validated card play.</summary>
    protected override void ApplyPlayCard(PlayerState player, PileState source, CardState card, PileState target)
    {
        if (State.Phase.Id == GamePhase.DealerDiscard) {
            MoveCard(source, card, target, faceUp: false);
            BeginPlay();
            return;
        }

        MoveCard(source, card, target, faceUp: true);
        var tricks = State.Tricks!;
        tricks.Plays.Add(new TrickPlayState {
            Seat = player.Seat,
            CardInstanceId = card.InstanceId!,
            Order = tricks.Plays.Count
        });
        var contract = State.Contract!;
        tricks.LedSuit ??= GetEffectiveSuit(card, GetTrumpSuit(contract));

        var cardsRequired = contract.AloneSeat is null ? 4 : 3;
        if (tricks.Plays.Count < cardsRequired) {
            var skipped = contract.AloneSeat is null
                ? null
                : new HashSet<int>([contract.PartnerSeat!.Value]);
            SetActiveSeat(NextSeat(player.Seat, skipped));
            return;
        }

        CompleteTrick();
    }

    private void DealHand()
    {
        InitializeRandomForDeal();
        CollectAllCards(PileIds.Deck);
        ShufflePile(PileIds.Deck);
        State.Contract = null;
        State.Auction = new AuctionState { Round = 1 };
        State.Melds.Clear();
        State.Tricks = CreateTricksState();
        ResetDealScores();
        SetVariable<string?>(EuchreVariableNames.UpcardInstanceId, null);
        SetVariable(EuchreVariableNames.BiddingRound, 1);
        SetVariable<int?>(EuchreVariableNames.MakerSeat, null);
        SetVariable<CardSuit?>(EuchreVariableNames.Trump, null);
        SetVariable(EuchreVariableNames.NoTrump, false);
        SetVariable<int?>(EuchreVariableNames.AloneSeat, null);
        SetVariable<int?>(EuchreVariableNames.InactiveSeat, null);
        SetVariable(EuchreVariableNames.OrderedUp, false);
        SetVariable(EuchreVariableNames.TricksPlayed, 0);
        State.Flags[GameFlag.FirstTrick] = true;

        DealRoundRobin(PileIds.Deck, 5);
        var deck = GetPile(PileIds.Deck);
        var kitty = GetPile(PileIds.Kitty);
        while (deck.Cards.Count > 0) {
            MoveCard(deck, TakeTop(deck), kitty, faceUp: false);
        }
        kitty.Cards[0].FaceUp = true;
        SetVariable(EuchreVariableNames.UpcardInstanceId, kitty.Cards[0].InstanceId);

        var firstBidder = NextSeat(State.DealerSeat!.Value);
        State.Auction.ActiveSeat = firstBidder;
        SetPhase(GamePhase.BidRoundOne, TurnMode.ExclusiveTurn, firstBidder);
    }

    private void ApplyPass(PlayerState player)
    {
        var auction = State.Auction!;
        if (State.Phase.Id == GamePhase.BidRoundTwo && player.Seat == State.DealerSeat
            && GetOption<bool>(EuchreOptionNames.StickTheDealer)
            && !GetOption<bool>(EuchreOptionNames.Ankle)) {
            throw Rule(GameRuleError.DealerMustBid, "Stick-the-dealer requires the dealer to choose trump.");
        }

        auction.Calls.Add(new BidState { Seat = player.Seat, Kind = BidKind.Pass, Revision = State.Revision + 1 });
        if (State.Phase.Id == GamePhase.BidRoundOne && player.Seat == State.DealerSeat) {
            auction.Round = 2;
            auction.PassedSeats.Clear();
            SetVariable(EuchreVariableNames.BiddingRound, 2);
            var firstBidder = NextSeat(State.DealerSeat!.Value);
            auction.ActiveSeat = firstBidder;
            SetPhase(GamePhase.BidRoundTwo, TurnMode.ExclusiveTurn, firstBidder);
            return;
        }
        if (State.Phase.Id == GamePhase.BidRoundTwo && player.Seat == State.DealerSeat) {
            State.DealerSeat = NextSeat(State.DealerSeat.Value);
            State.DealNumber++;
            DealHand();
            return;
        }

        var next = NextSeat(player.Seat);
        auction.ActiveSeat = next;
        SetActiveSeat(next);
    }

    private void ApplyOrderUp(PlayerState player, bool alone)
    {
        var upcard = GetCard(GetVariable<string>(EuchreVariableNames.UpcardInstanceId)!);
        var suit = upcard.Suit!;
        State.Auction!.Calls.Add(new BidState {
            Seat = player.Seat,
            Kind = BidKind.OrderUp,
            Suit = suit,
            Alone = alone,
            Revision = State.Revision + 1
        });
        State.Auction.HighCall = State.Auction.Calls[^1];
        State.Auction.Complete = true;
        CreateContract(player.Seat, suit!.Value, alone, orderedUp: true, TrumpKind.Suit);
        MoveCard(GetPile(PileIds.Kitty), upcard, GetPile(PileIds.Hand(State.DealerSeat!.Value)), faceUp: false);
        SetPhase(GamePhase.DealerDiscard, TurnMode.ExclusiveTurn, State.DealerSeat.Value);
    }

    private void ApplyChooseTrump(PlayerState player, CardSuit suit, bool alone)
    {
        var upcard = GetCard(GetVariable<string>(EuchreVariableNames.UpcardInstanceId)!);
        if (upcard.Suit == suit) {
            throw Rule(GameRuleError.InvalidTrump, "The turned suit may not be selected in the second bidding round.");
        }
        State.Auction!.Calls.Add(new BidState {
            Seat = player.Seat,
            Kind = BidKind.ChooseTrump,
            Suit = suit,
            Alone = alone,
            Revision = State.Revision + 1
        });
        State.Auction.HighCall = State.Auction.Calls[^1];
        State.Auction.Complete = true;
        CreateContract(player.Seat, suit, alone, orderedUp: false, TrumpKind.Suit);
        BeginPlay();
    }

    private void ApplyChooseNoTrump(PlayerState player, bool alone)
    {
        if (!GetOption<bool>(EuchreOptionNames.Ankle) || player.Seat != State.DealerSeat) {
            throw Rule(GameRuleError.InvalidNoTrump, "Only the final bidder may choose no trump when ankle is enabled.");
        }

        var call = new BidState {
            Seat = player.Seat,
            Kind = BidKind.ChooseNoTrump,
            Alone = alone,
            Revision = State.Revision + 1
        };
        State.Auction!.Calls.Add(call);
        State.Auction.HighCall = call;
        State.Auction.Complete = true;
        CreateContract(player.Seat, null, alone, orderedUp: false, TrumpKind.NoTrump);
        BeginPlay();
    }

    private void CreateContract(
        int makerSeat,
        CardSuit? suit,
        bool alone,
        bool orderedUp,
        TrumpKind trumpKind)
    {
        var partner = OppositeSeat(makerSeat);
        State.Contract = new ContractState {
            Kind = ContractKind.Euchre,
            DeclarerSeat = makerSeat,
            TeamId = GetTeamId(makerSeat),
            TrumpKind = trumpKind,
            Suit = suit,
            PartnerSeat = partner,
            AloneSeat = alone ? makerSeat : null
        };
        SetVariable(EuchreVariableNames.MakerSeat, makerSeat);
        SetVariable(EuchreVariableNames.Trump, suit);
        SetVariable(EuchreVariableNames.NoTrump, trumpKind == TrumpKind.NoTrump);
        SetVariable<int?>(EuchreVariableNames.AloneSeat, alone ? makerSeat : null);
        SetVariable<int?>(EuchreVariableNames.InactiveSeat, alone ? partner : null);
        SetVariable(EuchreVariableNames.OrderedUp, orderedUp);
    }

    private void ApplyAceNoFace(PlayerState player)
    {
        if (!GetOption<bool>(EuchreOptionNames.AceNoFaceRedeal) || !IsAceNoFace(GetPile(PileIds.Hand(player.Seat)).Cards)) {
            throw Rule(GameRuleError.InvalidAceNoFace, "The option is disabled or the hand is not exactly one ace with no face cards.");
        }
        State.DealNumber++;
        DealHand();
    }

    private void BeginPlay()
    {
        var skipped = State.Contract!.AloneSeat is null
            ? null
            : new HashSet<int>([State.Contract.PartnerSeat!.Value]);
        var leader = NextSeat(State.DealerSeat!.Value, skipped);
        State.Tricks = CreateTricksState();
        State.Tricks.LeaderSeat = leader;
        State.Turn.LeadSeat = leader;
        SetPhase(GamePhase.Play, TurnMode.ExclusiveTurn, leader);
    }

    private void CompleteTrick()
    {
        var tricks = State.Tricks!;
        var trump = GetTrumpSuit(State.Contract!);
        var winningPlay = tricks.Plays
            .OrderByDescending(play => GetCardStrength(GetCard(play.CardInstanceId), tricks.LedSuit!.Value, trump))
            .ThenBy(play => play.Order)
            .First();
        var winnerSeat = winningPlay.Seat;
        var winnerTeam = GetTeamId(winnerSeat)!.Value;
        tricks.Completed.Add(new CompletedTrickState {
            Number = tricks.Completed.Count,
            LeaderSeat = tricks.LeaderSeat!.Value,
            WinnerSeat = winnerSeat,
            WinningCardInstanceId = winningPlay.CardInstanceId,
            Plays = tricks.Plays.Select(ClonePlay).ToList()
        });
        tricks.WonBySeat[winnerSeat]++;
        tricks.WonByTeam[winnerTeam]++;
        var currentTrick = GetPile(PileIds.CurrentTrick);
        var won = GetPile(PileIds.Won(winnerTeam));
        foreach (var playedCard in currentTrick.Cards.ToArray()) {
            MoveCard(currentTrick, playedCard, won, faceUp: true);
        }
        SetVariable(EuchreVariableNames.TricksPlayed, tricks.Completed.Count);

        if (tricks.Completed.Count == 5) {
            ScoreCurrentDeal();
            return;
        }

        tricks.Plays.Clear();
        tricks.LedSuit = null;
        tricks.LeaderSeat = winnerSeat;
        State.Turn.LeadSeat = winnerSeat;
        State.Flags[GameFlag.FirstTrick] = false;
        SetPhase(GamePhase.Play, TurnMode.ExclusiveTurn, winnerSeat);
    }

    private void ScoreCurrentDeal()
    {
        SetPhase(GamePhase.ScoreDeal, TurnMode.Automatic);
        var makerTeam = State.Contract!.TeamId!.Value;
        var defenderTeam = State.Teams.Single(team => team.TeamId != makerTeam).TeamId;
        var makerTricks = State.Tricks!.WonByTeam[makerTeam];
        var points = ScoreDeal(makerTricks, State.Contract.AloneSeat is not null);
        State.Scores.Teams[makerTeam].Deal = points.MakerPoints;
        State.Scores.Teams[defenderTeam].Deal = points.DefenderPoints;
        State.Scores.Teams[makerTeam].Total += points.MakerPoints;
        State.Scores.Teams[defenderTeam].Total += points.DefenderPoints;

        var winner = State.Scores.Teams.FirstOrDefault(pair => pair.Value.Total >= State.Scores.Target);
        if (winner.Value is not null) {
            State.Status = GameStatus.Completed;
            State.Result = new GameResultState {
                WinnerTeamIds = [winner.Key],
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
        }
    }

    private void SetActiveSeat(int seat)
    {
        State.Turn.ActiveSeats = [seat];
        State.Turn.PrioritySeat = seat;
        if (State.Auction is not null && State.Phase.Id is GamePhase.BidRoundOne or GamePhase.BidRoundTwo) {
            State.Auction.ActiveSeat = seat;
        }
    }

    private int GetSeat(string playerId) => GetPlayer(playerId).Seat;

    private int OppositeSeat(int seat) => NextSeat(NextSeat(seat));

    private static CardSuit SameColorSuit(CardSuit suit) => suit switch {
        CardSuit.Clubs => CardSuit.Spades,
        CardSuit.Spades => CardSuit.Clubs,
        CardSuit.Diamonds => CardSuit.Hearts,
        CardSuit.Hearts => CardSuit.Diamonds,
        _ => throw new ArgumentOutOfRangeException(nameof(suit))
    };

    private static CardSuit? GetTrumpSuit(ContractState contract)
        => contract.TrumpKind switch {
            TrumpKind.Suit => contract.Suit
                ?? throw new InvalidOperationException("A suit-trump contract must name a suit."),
            TrumpKind.NoTrump => null,
            _ => throw new InvalidOperationException("The contract does not identify its trump mode.")
        };

    private static TrickPlayState ClonePlay(TrickPlayState play)
        => new() { Seat = play.Seat, CardInstanceId = play.CardInstanceId, Order = play.Order };

    private static GameActionRequest Request(string playerId, GameActionType name, object arguments, string actionId, long? revision)
        => new(playerId, name, JsonSerializer.SerializeToElement(arguments), actionId, revision);

    private static bool GetBoolean(JsonElement arguments, string property)
        => arguments.TryGetProperty(property, out var value) && value.GetBoolean();

    private static T GetEnum<T>(JsonElement arguments, string property) where T : struct, Enum
        => arguments.TryGetProperty(property, out var value)
            && value.ValueKind == JsonValueKind.String
            && Enum.TryParse<T>(value.GetString(), ignoreCase: true, out var result)
                ? result
                : throw Rule(GameRuleError.InvalidArguments, $"Valid {typeof(T).Name} argument '{property}' is required.");
}
