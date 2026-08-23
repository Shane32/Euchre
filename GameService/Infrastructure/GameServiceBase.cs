using System.Buffers.Binary;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace GameService.Infrastructure;

/// <summary>Provides shared functionality for card game services.</summary>
public abstract class GameServiceBase : IGameService
{
    private const string CardInstanceIdPrefix = "card-";
    private const string ViewerCardReferencePrefix = "known-";
    private const string UnknownCardReference = "unknown";

    private static readonly JsonSerializerOptions s_jsonOptions = new() {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        Converters =
        {
            new CardSymbolJsonConverter(),
            new System.Text.Json.Serialization.JsonStringEnumConverter(JsonNamingPolicy.CamelCase, allowIntegerValues: false)
        }
    };

    private readonly object _sync = new();
    private readonly Dictionary<RecordedActionKey, RecordedAction> _recordedActions = [];
    private readonly List<GameHistoryEntry> _historyEntries = [];
    private readonly Guid _baseSeed;
    private ReplayableRandom? _random;
    private List<Guid>? _activeActionRandomSeeds;
    private Queue<Guid>? _expectedReplayRandomSeeds;
    private DateTimeOffset? _activeActionTimestamp;
    private DateTimeOffset? _nextReplayTimestamp;

    /// <summary>Initializes a new instance of the game service base type.</summary>
    protected GameServiceBase(
        GameDefinition definition,
        string gameId,
        IReadOnlyList<PlayerRegistration> players,
        Guid? seed = null,
        IReadOnlyDictionary<string, object?>? options = null)
    {
        Definition = definition ?? throw new ArgumentNullException(nameof(definition));
        ArgumentException.ThrowIfNullOrWhiteSpace(gameId);
        ValidatePlayers(players);
        _baseSeed = seed ?? Guid.NewGuid();

        var now = DateTimeOffset.UtcNow;
        State = new GameState {
            GameId = gameId,
            GameType = definition.GameType,
            RulesVersion = definition.RulesVersion,
            CreatedAt = now,
            UpdatedAt = now,
            Players = CreatePlayers(players),
            Teams = definition.PlayerConfiguration.Teams.Select(team => new TeamState {
                TeamId = team.TeamId,
                SeatIds = [.. team.SeatIds]
            }).ToList(),
            Options = CreateOptions(options),
            Piles = CreatePiles(),
            ServerRandom = new ServerRandomState { BaseSeed = _baseSeed }
        };

        foreach (var team in State.Teams) {
            State.Scores.Teams[team.TeamId] = new ScoreEntryState();
        }
        foreach (var player in State.Players) {
            State.Scores.Players[player.PlayerId] = new ScoreEntryState();
            State.ServerKnowledge.Viewers[player.PlayerId] = new ViewerKnowledgeState();
        }
        foreach (var variable in definition.StateVariables) {
            State.Variables[variable.Name] = variable.Default.Clone();
        }

        PopulateInitialCards();
        RefreshKnowledge();
    }

    /// <summary>Gets the definition.</summary>
    public GameDefinition Definition { get; }

    /// <summary>Gets the state.</summary>
    protected GameState State { get; }

    /// <summary>Gets the sync root.</summary>
    protected object SyncRoot => _sync;

    /// <summary>Gets the random sequence for the current deal.</summary>
    protected ReplayableRandom Random
        => _random ?? throw new InvalidOperationException("Random must be initialized for the current deal.");

    /// <summary>Gets the timestamp of the action currently being executed.</summary>
    protected DateTimeOffset ActionTimestamp
        => _activeActionTimestamp ?? throw new InvalidOperationException("An action is not currently executing.");

    /// <summary>Starts the game.</summary>
    public abstract void StartGame();

    /// <summary>Gets the recorded game history.</summary>
    public GameHistory GetHistory()
    {
        lock (_sync) {
            return new GameHistory {
                GameId = State.GameId,
                GameType = State.GameType,
                RulesVersion = State.RulesVersion,
                BaseRandomSeed = _baseSeed,
                CreatedAt = State.CreatedAt,
                Players = State.Players
                    .Select(player => new PlayerRegistration(player.PlayerId, player.DisplayName))
                    .ToList(),
                Options = State.Options.ToDictionary(
                    pair => pair.Key,
                    pair => pair.Value.Clone(),
                    StringComparer.Ordinal),
                Entries = _historyEntries.Select(CloneHistoryEntry).ToList()
            };
        }
    }

    /// <summary>Gets the game state visible to a player.</summary>
    public GameState GetGameState(string playerId)
    {
        lock (_sync) {
            var player = GetPlayer(playerId);
            RefreshKnowledge();
            var projected = Clone(State);
            projected.ServerPendingDecisions.Clear();
            projected.ServerKnowledge = new ServerKnowledgeState();
            projected.ServerRandom = new ServerRandomState();

            var visiblePiles = new List<PileState>();
            foreach (var pile in projected.Piles) {
                var fullPile = GetPile(pile.PileId);
                var definition = GetPileDefinition(fullPile.DefinitionId);
                if (!CanSee(definition.Visibility.Existence, player, fullPile, null)) {
                    continue;
                }

                var canSeeCount = CanSee(definition.Visibility.Count, player, fullPile, null);
                pile.Count = canSeeCount ? fullPile.Cards.Count : null;
                if (!canSeeCount && !CanSeeAnyCard(definition.Visibility.Cards, player, fullPile)) {
                    pile.Cards.Clear();
                    visiblePiles.Add(pile);
                    continue;
                }

                pile.Cards = fullPile.Cards.Select(card => ProjectCard(player, fullPile, definition, card)).ToList();
                visiblePiles.Add(pile);
            }
            projected.Piles = visiblePiles;
            ProjectCardReferences(projected, player.PlayerId);
            return projected;
        }
    }

    /// <summary>Gets a stable hash of the game state visible to a player.</summary>
    public string GetGameHash(string playerId)
    {
        var json = JsonSerializer.Serialize(GetGameState(playerId), s_jsonOptions);
        return Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(json)))
            .TrimEnd('=').Replace('+', '-').Replace('/', '_');
    }

    /// <summary>Gets the actions currently available to a player.</summary>
    public IReadOnlyList<GameActionType> GetLegalActions(string playerId)
    {
        lock (_sync) {
            var player = GetPlayer(playerId);
            var phase = Definition.Phases.SingleOrDefault(item => item.Id == State.Phase.Id);
            if (phase is null || !IsActorAllowed(phase.Actors, player)) {
                return [];
            }

            return phase.AllowedActions
                .Where(action => IsActionAvailable(player, action))
                .ToArray();
        }
    }

    /// <summary>Attempts to play a card.</summary>
    public ActionResult PlayCard(PlayCardRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        var fingerprint = JsonSerializer.Serialize(request, s_jsonOptions);
        var historyAction = new PendingHistoryAction {
            Transport = ActionTransport.PlayCard,
            PlayerId = request.PlayerId,
            ExpectedRevision = request.ExpectedRevision,
            SourcePileId = request.SourcePileId,
            CardIndex = request.CardIndex,
            TargetPileId = request.TargetPileId
        };
        return ExecuteAction(request.PlayerId, request.ActionId, request.ExpectedRevision, fingerprint, historyAction, () => {
            var player = GetPlayer(request.PlayerId);
            var playAction = Definition.Actions.SingleOrDefault(action =>
                action.Transport == ActionTransport.PlayCard && action.Phases.Contains(State.Phase.Id))
                ?? throw Rule(GameRuleError.InvalidPhase, $"No card action is allowed during phase '{State.Phase.Id}'.");
            historyAction.Action = playAction.Id;
            ValidateActionDefinition(player, playAction.Id, ActionTransport.PlayCard);
            var source = GetPile(request.SourcePileId);
            var target = GetPile(request.TargetPileId);
            if (request.CardIndex < 0 || request.CardIndex >= source.Cards.Count) {
                throw Rule(GameRuleError.InvalidCard, "The selected card index is not present in the source pile.");
            }
            var card = source.Cards[request.CardIndex];
            ValidatePlayCard(player, source, card, target);
            ApplyPlayCard(player, source, card, target);
        });
    }

    /// <summary>Attempts to perform a game action.</summary>
    public ActionResult PerformAction(GameActionRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        var fingerprint = JsonSerializer.Serialize(request, s_jsonOptions);
        var historyAction = new PendingHistoryAction {
            Transport = ActionTransport.Other,
            Action = request.Name,
            PlayerId = request.PlayerId,
            ExpectedRevision = request.ExpectedRevision,
            Arguments = request.Arguments.Clone()
        };
        return ExecuteAction(request.PlayerId, request.ActionId, request.ExpectedRevision, fingerprint, historyAction, () => {
            var player = GetPlayer(request.PlayerId);
            ValidateActionDefinition(player, request.Name, ActionTransport.Other);
            if (request.Name == GameActionType.Ready) {
                player.Ready = true;
            } else {
                ApplyOtherAction(player, request.Name, request.Arguments);
            }
        });
    }

    /// <summary>Determines whether an action is available to a player.</summary>
    protected virtual bool IsActionAvailable(PlayerState player, GameActionType actionId) => true;

    /// <summary>Validates a requested card play.</summary>
    protected virtual void ValidatePlayCard(PlayerState player, PileState source, CardState card, PileState target)
    {
        if (source.Owner?.Kind != PileOwnerKind.Seat || source.Owner.Seat != player.Seat) {
            throw Rule(GameRuleError.NotAuthorized, "A player may only play from their own pile.");
        }
        if (State.Turn.ActiveSeats.Count > 0 && !State.Turn.ActiveSeats.Contains(player.Seat)) {
            throw Rule(GameRuleError.NotYourTurn, "It is not this player's turn.");
        }
    }

    /// <summary>Applies a validated card play.</summary>
    protected virtual void ApplyPlayCard(PlayerState player, PileState source, CardState card, PileState target)
    {
        MoveCard(source, card, target, faceUp: true);
    }

    /// <summary>Applies a non-card game action.</summary>
    protected virtual void ApplyOtherAction(PlayerState player, GameActionType name, JsonElement arguments)
    {
        throw Rule(GameRuleError.UnsupportedAction, $"Action '{name}' is not implemented by {GetType().Name}.");
    }

    /// <summary>Replays a recorded system action.</summary>
    protected virtual void ReplaySystemAction(GameHistoryEntry entry)
    {
        if (entry.Sequence == 1 && entry.Action == GameActionType.Deal) {
            StartGame();
            return;
        }
        throw new InvalidOperationException($"System action '{entry.Action}' is not supported by {GetType().Name} history replay.");
    }

    /// <summary>Executes and records a system action.</summary>
    protected ActionResult ExecuteSystemAction(GameActionType action, string actionId, Action mutation)
        => ExecuteAction(
            SystemActionIds.Actor,
            actionId,
            null,
            actionId,
            new PendingHistoryAction { Transport = ActionTransport.System, Action = action },
            mutation,
            validatePlayer: false);

    /// <summary>Sets the current game phase and active seats.</summary>
    protected void SetPhase(GamePhase phaseId, TurnMode mode, params int[] activeSeats)
    {
        if (!Definition.Phases.Any(phase => phase.Id == phaseId)) {
            throw new InvalidOperationException($"Definition does not contain phase '{phaseId}'.");
        }

        State.Phase = new PhaseState {
            Id = phaseId,
            Step = State.Phase.Id == phaseId ? State.Phase.Step + 1 : 0,
            StartedAt = ActionTimestamp
        };
        State.Turn.Mode = mode;
        State.Turn.ActiveSeats = [.. activeSeats];
        State.Turn.PrioritySeat = activeSeats.FirstOrDefault();
    }

    /// <summary>Gets a player by identifier.</summary>
    protected PlayerState GetPlayer(string playerId)
        => State.Players.SingleOrDefault(player => player.PlayerId == playerId)
            ?? throw Rule(GameRuleError.PlayerNotFound, $"Player '{playerId}' does not belong to this game.");

    /// <summary>Gets the player occupying a seat.</summary>
    protected PlayerState GetPlayerBySeat(int seat)
        => State.Players.Single(player => player.Seat == seat);

    /// <summary>Gets the player identifier for a seat.</summary>
    protected string GetPlayerId(int seat) => GetPlayerBySeat(seat).PlayerId;

    /// <summary>Gets the team identifier for a seat.</summary>
    protected int? GetTeamId(int seat)
        => State.Teams.SingleOrDefault(team => team.SeatIds.Contains(seat))?.TeamId;

    /// <summary>Gets the next eligible seat in turn order.</summary>
    protected int NextSeat(int seat, ISet<int>? skippedSeats = null)
    {
        var order = Definition.PlayerConfiguration.TurnOrder;
        var index = order.IndexOf(seat);
        if (index < 0) {
            throw new InvalidOperationException($"Unknown seat '{seat}'.");
        }

        for (var offset = 1; offset <= order.Count; offset++) {
            var candidate = order[(index + offset) % order.Count];
            if (skippedSeats is null || !skippedSeats.Contains(candidate)) {
                return candidate;
            }
        }
        throw new InvalidOperationException("No active seat remains.");
    }

    /// <summary>Gets a pile by identifier.</summary>
    protected PileState GetPile(PileId pileId)
        => State.Piles.SingleOrDefault(pile => pile.PileId == pileId)
            ?? throw Rule(GameRuleError.PileNotFound, $"Pile '{pileId}' does not exist.");

    /// <summary>Gets a card by instance identifier.</summary>
    protected CardState GetCard(string instanceId)
        => State.Piles.SelectMany(pile => pile.Cards).Single(card => card.InstanceId == instanceId);

    /// <summary>Gets the pile containing a card.</summary>
    protected PileState GetCardPile(string instanceId)
        => State.Piles.Single(pile => pile.Cards.Any(card => card.InstanceId == instanceId));

    /// <summary>Moves a card between piles.</summary>
    protected void MoveCard(PileState source, CardState card, PileState target, bool? faceUp = null)
    {
        if (!source.Cards.Remove(card)) {
            throw new InvalidOperationException($"Card '{card.InstanceId}' is not in pile '{source.PileId}'.");
        }
        if (faceUp.HasValue) {
            card.FaceUp = faceUp.Value;
        }
        target.Cards.Add(card);
    }

    /// <summary>
    /// Starts a fresh fixed-algorithm random sequence for a deal and associates
    /// its seed with the action currently being recorded.
    /// </summary>
    protected Guid InitializeRandomForDeal(Guid? seed = null)
    {
        Guid effectiveSeed;
        if (_expectedReplayRandomSeeds is not null) {
            if (_expectedReplayRandomSeeds.Count == 0) {
                throw new InvalidOperationException("Replay history did not provide a seed for a deal that uses randomness.");
            }
            effectiveSeed = _expectedReplayRandomSeeds.Dequeue();
            if (seed.HasValue && seed.Value != effectiveSeed) {
                throw new InvalidOperationException("The replayed deal seed does not match the recorded seed.");
            }
        } else {
            effectiveSeed = seed ?? CreateDealSeed(State.ServerRandom.DealSequence);
        }

        _random = new ReplayableRandom(effectiveSeed);
        State.ServerRandom.CurrentDealSeed = effectiveSeed;
        State.ServerRandom.DealSequence++;
        _activeActionRandomSeeds?.Add(effectiveSeed);
        return effectiveSeed;
    }

    /// <summary>Shuffles a pile using the current random sequence.</summary>
    protected void ShufflePile(PileId pileId)
    {
        var pile = GetPile(pileId);
        State.ServerRandom.ShuffleNumber++;
        for (var index = pile.Cards.Count - 1; index > 0; index--) {
            var other = Random.Next(index + 1);
            (pile.Cards[index], pile.Cards[other]) = (pile.Cards[other], pile.Cards[index]);
        }
        State.ServerKnowledge.Epoch++;
        foreach (var knowledge in State.ServerKnowledge.Viewers.Values) {
            foreach (var card in pile.Cards) {
                if (card.InstanceId is not null) {
                    knowledge.KnownCardInstanceIds.Remove(card.InstanceId);
                    knowledge.CardRefs.Remove(card.InstanceId);
                }
            }
        }
    }

    /// <summary>Creates a JSON element for a value.</summary>
    protected static JsonElement JsonValue<T>(T value) => JsonSerializer.SerializeToElement(value, s_jsonOptions);

    /// <summary>Gets a configured game option.</summary>
    protected T GetOption<T>(string name)
    {
        if (!State.Options.TryGetValue(name, out var value)) {
            throw new InvalidOperationException($"Option '{name}' is not defined.");
        }
        return value.Deserialize<T>(s_jsonOptions)!;
    }

    /// <summary>Gets a game state variable.</summary>
    protected T? GetVariable<T>(string name)
    {
        if (!State.Variables.TryGetValue(name, out var value) || value.ValueKind == JsonValueKind.Null) {
            return default;
        }
        return value.Deserialize<T>(s_jsonOptions);
    }

    /// <summary>Sets a game state variable.</summary>
    protected void SetVariable<T>(string name, T value) => State.Variables[name] = JsonValue(value);

    /// <summary>Creates a game-rule exception.</summary>
    protected static GameRuleException Rule(GameRuleError code, string message) => new(code, message);

    /// <summary>Deals cards to each player in turn.</summary>
    protected void DealRoundRobin(PileId deckPileId, int cardsPerPlayer)
    {
        var deck = GetPile(deckPileId);
        for (var cardNumber = 0; cardNumber < cardsPerPlayer; cardNumber++) {
            foreach (var seat in Definition.PlayerConfiguration.TurnOrder) {
                var card = TakeTop(deck);
                MoveCard(deck, card, GetPile(PileIds.Hand(seat)), faceUp: false);
            }
        }
    }

    /// <summary>Removes and returns the top card of a pile.</summary>
    protected CardState TakeTop(PileState pile)
    {
        var definition = GetPileDefinition(pile.DefinitionId);
        if (pile.Cards.Count == 0) {
            throw new InvalidOperationException($"Pile '{pile.PileId}' is empty.");
        }
        return definition.Top == PileTop.First ? pile.Cards[0] : pile.Cards[^1];
    }

    /// <summary>Collects all cards into a target pile.</summary>
    protected void CollectAllCards(PileId targetPileId)
    {
        var target = GetPile(targetPileId);
        foreach (var pile in State.Piles.Where(pile => pile != target).ToArray()) {
            foreach (var card in pile.Cards.ToArray()) {
                MoveCard(pile, card, target, faceUp: false);
            }
        }
    }

    /// <summary>Gets a clone of the authoritative game state.</summary>
    protected GameState GetAuthoritativeStateClone()
    {
        lock (_sync) {
            return Clone(State);
        }
    }

    /// <summary>Replays recorded game history.</summary>
    protected void ReplayHistory(GameHistory history)
    {
        ArgumentNullException.ThrowIfNull(history);
        lock (_sync) {
            ValidateHistoryHeader(history);
            if (State.Revision != 0 || _historyEntries.Count != 0) {
                throw new InvalidOperationException("History can only be replayed into a new game service.");
            }

            State.CreatedAt = history.CreatedAt;
            State.UpdatedAt = history.CreatedAt;
            for (var index = 0; index < history.Entries.Count; index++) {
                var entry = history.Entries[index];
                var expectedSequence = index + 1L;
                if (entry.Sequence != expectedSequence || entry.Revision != expectedSequence) {
                    throw new InvalidOperationException("History sequence and revision numbers must be contiguous and start at one.");
                }

                _nextReplayTimestamp = entry.OccurredAt;
                _expectedReplayRandomSeeds = new Queue<Guid>(entry.RandomSeeds);
                switch (entry.Transport) {
                    case ActionTransport.System:
                        ReplaySystemAction(entry);
                        break;
                    case ActionTransport.Other:
                        if (entry.PlayerId is null || entry.Arguments is null) {
                            throw new InvalidOperationException("An other-action history entry requires a player and arguments.");
                        }
                        PerformAction(new GameActionRequest(
                            entry.PlayerId,
                            entry.Action,
                            entry.Arguments.Value.Clone(),
                            entry.ActionId,
                            entry.ExpectedRevision));
                        break;
                    case ActionTransport.PlayCard:
                        if (entry.PlayerId is null
                            || entry.SourcePileId is null
                            || entry.CardIndex is null
                            || entry.TargetPileId is null) {
                            throw new InvalidOperationException("A play-card history entry is incomplete.");
                        }
                        PlayCard(new PlayCardRequest(
                            entry.PlayerId,
                            entry.SourcePileId.Value,
                            entry.CardIndex.Value,
                            entry.TargetPileId.Value,
                            entry.ActionId,
                            entry.ExpectedRevision));
                        break;
                    default:
                        throw new InvalidOperationException($"History entry {entry.Sequence} is not a supported replay command.");
                }

                var replayed = _historyEntries[^1];
                if (!HistoryEntriesMatch(entry, replayed)) {
                    throw new InvalidOperationException($"History entry {entry.Sequence} did not replay to the recorded command and state transition.");
                }
            }
        }
    }

    /// <summary>Gets the options stored in recorded game history.</summary>
    protected static IReadOnlyDictionary<string, object?> GetReplayOptions(GameHistory history)
        => history.Options.ToDictionary(
            pair => pair.Key,
            pair => (object?)pair.Value.Clone(),
            StringComparer.Ordinal);

    private ActionResult ExecuteAction(
        string actorId,
        string actionId,
        long? expectedRevision,
        string fingerprint,
        PendingHistoryAction historyAction,
        Action mutation,
        bool validatePlayer = true)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(actionId);
        lock (_sync) {
            if (validatePlayer) {
                GetPlayer(actorId);
            }
            var recordKey = new RecordedActionKey(actorId, actionId);
            if (_recordedActions.TryGetValue(recordKey, out var recorded)) {
                if (recorded.Fingerprint != fingerprint) {
                    throw Rule(GameRuleError.ActionIdConflict, "The action ID was already used with different content.");
                }
                return recorded.Result;
            }
            if (expectedRevision.HasValue && expectedRevision.Value != State.Revision) {
                throw Rule(GameRuleError.RevisionConflict, $"Expected revision {expectedRevision}, but current revision is {State.Revision}.");
            }

            var occurredAt = _nextReplayTimestamp ?? DateTimeOffset.UtcNow;
            _nextReplayTimestamp = null;
            _activeActionTimestamp = occurredAt;
            _activeActionRandomSeeds = [];
            try {
                mutation();
                if (_expectedReplayRandomSeeds is { Count: > 0 }) {
                    throw new InvalidOperationException("Replay history contains random seeds that were not consumed by the action.");
                }

                State.Revision++;
                State.UpdatedAt = occurredAt;
                RefreshKnowledge();
                var result = new ActionResult(true, actionId, State.Revision);
                _recordedActions.Add(recordKey, new RecordedAction(fingerprint, result));
                _historyEntries.Add(new GameHistoryEntry {
                    Sequence = _historyEntries.Count + 1,
                    Revision = State.Revision,
                    Transport = historyAction.Transport,
                    Action = historyAction.Action,
                    ActionId = actionId,
                    PlayerId = historyAction.PlayerId,
                    ExpectedRevision = historyAction.ExpectedRevision,
                    Arguments = historyAction.Arguments?.Clone(),
                    SourcePileId = historyAction.SourcePileId,
                    CardIndex = historyAction.CardIndex,
                    TargetPileId = historyAction.TargetPileId,
                    RandomSeeds = [.. _activeActionRandomSeeds],
                    OccurredAt = occurredAt
                });
                return result;
            } finally {
                _activeActionTimestamp = null;
                _activeActionRandomSeeds = null;
                _expectedReplayRandomSeeds = null;
            }
        }
    }

    private void ValidateActionDefinition(PlayerState player, GameActionType actionId, ActionTransport transport)
    {
        var action = Definition.Actions.SingleOrDefault(item => item.Id == actionId)
            ?? throw Rule(GameRuleError.UnsupportedAction, $"Action '{actionId}' is not defined.");
        if (action.Transport != transport) {
            throw Rule(GameRuleError.InvalidAction, $"Action '{actionId}' does not use transport '{transport}'.");
        }
        if (!action.Phases.Contains(State.Phase.Id)) {
            throw Rule(GameRuleError.InvalidPhase, $"Action '{actionId}' is not allowed during phase '{State.Phase.Id}'.");
        }
        if (!IsActorAllowed(action.Actor, player)) {
            throw Rule(GameRuleError.NotYourTurn, "The player is not an actor for this action.");
        }
    }

    private bool IsActorAllowed(ActorSelector selector, PlayerState player)
        => selector switch {
            ActorSelector.Self or ActorSelector.AllPlayers => true,
            ActorSelector.ActiveSeat => State.Turn.ActiveSeats.Contains(player.Seat),
            ActorSelector.Dealer => State.DealerSeat == player.Seat,
            ActorSelector.Declarer => State.Contract?.DeclarerSeat == player.Seat,
            ActorSelector.System or ActorSelector.None => false,
            _ => EvaluateActorSelector(selector, player)
        };

    /// <summary>Determines whether a player matches an actor selector.</summary>
    protected virtual bool EvaluateActorSelector(ActorSelector selector, PlayerState player) => false;

    private void ValidatePlayers(IReadOnlyList<PlayerRegistration> players)
    {
        ArgumentNullException.ThrowIfNull(players);
        var config = Definition.PlayerConfiguration;
        if (players.Count < config.MinimumPlayers || players.Count > config.MaximumPlayers) {
            throw new ArgumentException($"{Definition.DisplayName} requires {config.MinimumPlayers} to {config.MaximumPlayers} players.", nameof(players));
        }
        if (players.Select(player => player.PlayerId).Distinct(StringComparer.Ordinal).Count() != players.Count) {
            throw new ArgumentException("Player IDs must be unique.", nameof(players));
        }
    }

    private List<PlayerState> CreatePlayers(IReadOnlyList<PlayerRegistration> registrations)
    {
        var players = new List<PlayerState>(registrations.Count);
        for (var index = 0; index < registrations.Count; index++) {
            var registration = registrations[index];
            var seat = Definition.PlayerConfiguration.Seats[index];
            players.Add(new PlayerState {
                PlayerId = registration.PlayerId,
                DisplayName = registration.DisplayName,
                Seat = seat,
                TeamId = Definition.PlayerConfiguration.Teams
                    .SingleOrDefault(team => team.SeatIds.Contains(seat))?.TeamId
            });
        }
        return players;
    }

    private Dictionary<string, JsonElement> CreateOptions(IReadOnlyDictionary<string, object?>? supplied)
    {
        var values = new Dictionary<string, JsonElement>(StringComparer.Ordinal);
        foreach (var pair in Definition.Options) {
            var defaultValue = pair.Value.Default.Clone();
            ValidateOption(pair.Key, pair.Value, defaultValue);
            values[pair.Key] = defaultValue;
        }
        if (supplied is not null) {
            foreach (var pair in supplied) {
                if (!Definition.Options.ContainsKey(pair.Key)) {
                    throw new ArgumentException($"Unknown game option '{pair.Key}'.", nameof(supplied));
                }
                var value = JsonValue(pair.Value);
                ValidateOption(pair.Key, Definition.Options[pair.Key], value);
                values[pair.Key] = value;
            }
        }
        return values;
    }

    private static void ValidateOption(string name, OptionDefinition definition, JsonElement value)
    {
        var validType = definition.Type switch {
            OptionValueType.Boolean => value.ValueKind is JsonValueKind.True or JsonValueKind.False,
            OptionValueType.Integer => value.ValueKind == JsonValueKind.Number && value.TryGetInt32(out _),
            OptionValueType.String => value.ValueKind == JsonValueKind.String,
            _ => throw new InvalidOperationException($"Option '{name}' uses unsupported type '{definition.Type}'.")
        };
        if (!validType) {
            throw new ArgumentException($"Option '{name}' must be a {definition.Type}.", nameof(value));
        }

        if (definition.Type == OptionValueType.Integer) {
            var number = value.GetInt32();
            if (definition.Minimum.HasValue && number < definition.Minimum.Value
                || definition.Maximum.HasValue && number > definition.Maximum.Value) {
                throw new ArgumentException($"Option '{name}' is outside its allowed range.", nameof(value));
            }
        }
        if (definition.Enum is { Count: > 0 }
            && !definition.Enum.Contains(value.GetString()!, StringComparer.Ordinal)) {
            throw new ArgumentException($"Option '{name}' is not an allowed value.", nameof(value));
        }
    }

    private List<PileState> CreatePiles()
    {
        var piles = new List<PileState>();
        foreach (var definition in Definition.PileDefinitions) {
            switch (definition.Instantiate) {
                case PileInstantiation.Once:
                    piles.Add(CreatePile(definition, null, null));
                    break;
                case PileInstantiation.PerSeat:
                    piles.AddRange(Definition.PlayerConfiguration.Seats.Select(seat => CreatePile(definition, seat, null)));
                    break;
                case PileInstantiation.PerTeam:
                    piles.AddRange(Definition.PlayerConfiguration.Teams.Select(team => CreatePile(definition, null, team.TeamId)));
                    break;
                default:
                    throw new InvalidOperationException($"Unknown pile instantiation '{definition.Instantiate}'.");
            }
        }
        return piles;
    }

    private static PileState CreatePile(PileDefinition definition, int? seat, int? team)
    {
        var ownerId = definition.Instantiate switch {
            PileInstantiation.PerSeat => seat,
            PileInstantiation.PerTeam => team,
            _ => null
        };
        var owner = definition.Owner.HasValue
            ? new PileOwnerState {
                Kind = definition.Owner.Value,
                Seat = definition.Owner == PileOwnerKind.Seat ? seat : null,
                Team = definition.Owner == PileOwnerKind.Team ? team : null
            }
            : null;
        return new PileState {
            PileId = new PileId(definition.Id, ownerId),
            DefinitionId = definition.Id,
            Kind = definition.Kind,
            Owner = owner
        };
    }

    private void PopulateInitialCards()
    {
        var initialPileDefinition = Definition.PileDefinitions.SingleOrDefault(
            pile => pile.Initialization?.Cards == PileCardSource.All)
            ?? throw new InvalidOperationException("A pile initialized with all cards is required.");
        var pile = GetPile(new PileId(initialPileDefinition.Id));
        var instance = 0;
        if (Definition.CardSet.Construction.Kind == CardConstructionKind.CartesianProduct) {
            for (var copy = 0; copy < Definition.CardSet.Copies; copy++) {
                foreach (var suit in Definition.CardSet.Suits) {
                    foreach (var rank in Definition.CardSet.Ranks) {
                        var cardId = Definition.CardSet.Construction.CardIdFormat
                            .Replace("{rank}", rank.Id.ToString().ToLowerInvariant(), StringComparison.Ordinal)
                            .Replace("{suit}", suit.Id.ToString().ToLowerInvariant(), StringComparison.Ordinal);
                        pile.Cards.Add(new CardState {
                            InstanceId = $"{CardInstanceIdPrefix}{++instance:D3}",
                            CardId = cardId,
                            Copy = copy,
                            Rank = rank.Id,
                            Suit = suit.Id
                        });
                    }
                }
            }
        } else {
            foreach (var explicitCard in Definition.CardSet.Construction.Cards ?? []) {
                pile.Cards.Add(new CardState {
                    InstanceId = $"{CardInstanceIdPrefix}{++instance:D3}",
                    CardId = explicitCard.CardId,
                    Copy = 0,
                    Rank = explicitCard.Rank,
                    Suit = explicitCard.Suit
                });
            }
        }
    }

    private PileDefinition GetPileDefinition(PileType definitionId)
        => Definition.PileDefinitions.Single(definition => definition.Id == definitionId);

    private void RefreshKnowledge()
    {
        foreach (var player in State.Players) {
            var knowledge = State.ServerKnowledge.Viewers[player.PlayerId];
            foreach (var pile in State.Piles) {
                var definition = GetPileDefinition(pile.DefinitionId);
                foreach (var card in pile.Cards) {
                    if (card.InstanceId is not null
                        && (pile.Owner?.Kind == PileOwnerKind.Seat && pile.Owner.Seat == player.Seat
                            || card.FaceUp
                            || definition.Visibility.Cards == VisibilityPolicy.Public)) {
                        knowledge.KnownCardInstanceIds.Add(card.InstanceId);
                    }
                }
            }
        }
    }

    private bool CanSee(VisibilityPolicy policy, PlayerState player, PileState pile, CardState? card)
    {
        var knowledge = State.ServerKnowledge.Viewers[player.PlayerId];
        return policy switch {
            VisibilityPolicy.Public => true,
            VisibilityPolicy.None => false,
            VisibilityPolicy.Owner => pile.Owner?.Kind == PileOwnerKind.Seat && pile.Owner.Seat == player.Seat,
            VisibilityPolicy.Team => pile.Owner?.Kind == PileOwnerKind.Team && pile.Owner.Team == player.TeamId,
            VisibilityPolicy.Known => card?.InstanceId is not null && knowledge.KnownCardInstanceIds.Contains(card.InstanceId),
            VisibilityPolicy.OwnerOrKnown => pile.Owner?.Kind == PileOwnerKind.Seat && pile.Owner.Seat == player.Seat
                || card?.InstanceId is not null && knowledge.KnownCardInstanceIds.Contains(card.InstanceId),
            VisibilityPolicy.FaceUpOrKnown => card?.FaceUp == true
                || card?.InstanceId is not null && knowledge.KnownCardInstanceIds.Contains(card.InstanceId),
            VisibilityPolicy.VisibleCardPositions => true,
            _ => EvaluateVisibility(policy, player, pile, card)
        };
    }

    /// <summary>Determines whether a card is visible to a player.</summary>
    protected virtual bool EvaluateVisibility(VisibilityPolicy policy, PlayerState player, PileState pile, CardState? card) => false;

    private bool CanSeeAnyCard(VisibilityPolicy policy, PlayerState player, PileState pile)
        => pile.Cards.Any(card => CanSee(policy, player, pile, card));

    private CardState ProjectCard(PlayerState player, PileState fullPile, PileDefinition definition, CardState card)
    {
        if (!CanSee(definition.Visibility.Cards, player, fullPile, card)) {
            return new CardState { Unknown = true, FaceUp = card.FaceUp };
        }

        var knowledge = State.ServerKnowledge.Viewers[player.PlayerId];
        var instanceId = card.InstanceId!;
        if (!knowledge.CardRefs.TryGetValue(instanceId, out var reference)) {
            reference = CreateViewerCardReference(player.PlayerId, instanceId, State.ServerKnowledge.Epoch);
            knowledge.CardRefs.Add(instanceId, reference);
        }
        return new CardState {
            Ref = reference,
            CardId = card.CardId,
            Rank = card.Rank,
            Suit = card.Suit,
            FaceUp = card.FaceUp,
            Orientation = card.Orientation,
            Annotations = card.Annotations.ToDictionary(pair => pair.Key, pair => pair.Value.Clone(), StringComparer.Ordinal)
        };
    }

    private void ProjectCardReferences(GameState projected, string playerId)
    {
        var knowledge = State.ServerKnowledge.Viewers[playerId];
        string Map(string instanceId) => knowledge.CardRefs.TryGetValue(instanceId, out var reference) ? reference : UnknownCardReference;
        if (projected.Tricks is not null) {
            foreach (var play in projected.Tricks.Plays) {
                play.CardInstanceId = Map(play.CardInstanceId);
            }
            foreach (var trick in projected.Tricks.Completed) {
                trick.WinningCardInstanceId = Map(trick.WinningCardInstanceId);
                foreach (var play in trick.Plays) {
                    play.CardInstanceId = Map(play.CardInstanceId);
                }
            }
        }
        foreach (var meld in projected.Melds) {
            meld.CardInstanceIds = meld.CardInstanceIds.Select(Map).Where(reference => reference != UnknownCardReference).ToList();
        }
        foreach (var variable in Definition.StateVariables.Where(variable => variable.Type == StateVariableType.CardInstanceId)) {
            if (projected.Variables.TryGetValue(variable.Name, out var value) && value.ValueKind == JsonValueKind.String) {
                var instanceId = value.GetString()!;
                projected.Variables[variable.Name] = knowledge.KnownCardInstanceIds.Contains(instanceId)
                    ? JsonValue(Map(instanceId))
                    : JsonValue<string?>(null);
            }
        }
    }

    private static GameState Clone(GameState state)
        => JsonSerializer.Deserialize<GameState>(JsonSerializer.Serialize(state, s_jsonOptions), s_jsonOptions)!;

    private void ValidateHistoryHeader(GameHistory history)
    {
        if (history.SchemaVersion != GameHistorySchemaVersions.Version1) {
            throw new InvalidOperationException($"Unsupported history schema version '{history.SchemaVersion}'.");
        }
        if (history.GameId != State.GameId
            || history.GameType != State.GameType
            || history.RulesVersion != State.RulesVersion
            || history.BaseRandomSeed != _baseSeed) {
            throw new InvalidOperationException("History metadata does not match the game service.");
        }

        var playersMatch = history.Players.Count == State.Players.Count
            && history.Players.Zip(State.Players).All(pair =>
                pair.First.PlayerId == pair.Second.PlayerId
                && pair.First.DisplayName == pair.Second.DisplayName);
        if (!playersMatch) {
            throw new InvalidOperationException("History players do not match the game service.");
        }
        if (history.Options.Count != State.Options.Count
            || history.Options.Any(pair =>
                !State.Options.TryGetValue(pair.Key, out var value)
                || pair.Value.GetRawText() != value.GetRawText())) {
            throw new InvalidOperationException("History options do not match the game service.");
        }
    }

    private static GameHistoryEntry CloneHistoryEntry(GameHistoryEntry entry)
        => new() {
            Sequence = entry.Sequence,
            Revision = entry.Revision,
            Transport = entry.Transport,
            Action = entry.Action,
            ActionId = entry.ActionId,
            PlayerId = entry.PlayerId,
            ExpectedRevision = entry.ExpectedRevision,
            Arguments = entry.Arguments?.Clone(),
            SourcePileId = entry.SourcePileId,
            CardIndex = entry.CardIndex,
            TargetPileId = entry.TargetPileId,
            RandomSeeds = [.. entry.RandomSeeds],
            OccurredAt = entry.OccurredAt
        };

    private static bool HistoryEntriesMatch(GameHistoryEntry expected, GameHistoryEntry actual)
        => expected.Sequence == actual.Sequence
            && expected.Revision == actual.Revision
            && expected.Transport == actual.Transport
            && expected.Action == actual.Action
            && expected.ActionId == actual.ActionId
            && expected.PlayerId == actual.PlayerId
            && expected.ExpectedRevision == actual.ExpectedRevision
            && NullableJsonMatches(expected.Arguments, actual.Arguments)
            && expected.SourcePileId == actual.SourcePileId
            && expected.CardIndex == actual.CardIndex
            && expected.TargetPileId == actual.TargetPileId
            && expected.RandomSeeds.SequenceEqual(actual.RandomSeeds)
            && expected.OccurredAt == actual.OccurredAt;

    private static bool NullableJsonMatches(JsonElement? first, JsonElement? second)
        => first.HasValue == second.HasValue
            && (!first.HasValue || first.Value.GetRawText() == second!.Value.GetRawText());

    private static string CreateViewerCardReference(string playerId, string instanceId, long epoch)
    {
        var value = $"{playerId}\n{instanceId}\n{epoch}";
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(value));
        return ViewerCardReferencePrefix + Convert.ToBase64String(hash).TrimEnd('=').Replace('+', '-').Replace('/', '_');
    }

    private Guid CreateDealSeed(int dealSequence)
    {
        if (dealSequence == 0) {
            return _baseSeed;
        }

        var random = new ReplayableRandom(_baseSeed);
        Span<byte> seedBytes = stackalloc byte[16];
        for (var currentDeal = 1; currentDeal <= dealSequence; currentDeal++) {
            for (var offset = 0; offset < seedBytes.Length; offset += sizeof(int)) {
                BinaryPrimitives.WriteInt32LittleEndian(
                    seedBytes[offset..],
                    random.Next(int.MinValue, int.MaxValue));
            }
        }
        return new Guid(seedBytes);
    }

}
