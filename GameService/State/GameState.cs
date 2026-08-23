using System.Text.Json;

namespace GameService.State;

/// <summary>Represents the current game state.</summary>
public sealed class GameState
{
    /// <summary>Gets or sets the schema version.</summary>
    public string SchemaVersion { get; set; } = "card-game-state/1.0";
    /// <summary>Gets or sets the game identifier.</summary>
    public required string GameId { get; set; }
    /// <summary>Gets or sets the game type.</summary>
    public GameType GameType { get; set; }
    /// <summary>Gets or sets the rules version.</summary>
    public required string RulesVersion { get; set; }
    /// <summary>Gets or sets the options.</summary>
    public Dictionary<string, JsonElement> Options { get; set; } = new(StringComparer.Ordinal);
    /// <summary>Gets or sets the revision.</summary>
    public long Revision { get; set; }
    /// <summary>Gets or sets the creation timestamp.</summary>
    public DateTimeOffset CreatedAt { get; set; }
    /// <summary>Gets or sets the last update timestamp.</summary>
    public DateTimeOffset UpdatedAt { get; set; }
    /// <summary>Gets or sets the status.</summary>
    public GameStatus Status { get; set; } = GameStatus.Waiting;
    /// <summary>Gets or sets the round number.</summary>
    public int RoundNumber { get; set; }
    /// <summary>Gets or sets the deal number.</summary>
    public int DealNumber { get; set; }
    /// <summary>Gets or sets the phase.</summary>
    public PhaseState Phase { get; set; } = new() { Id = GamePhase.Waiting };
    /// <summary>Gets or sets the players.</summary>
    public List<PlayerState> Players { get; set; } = [];
    /// <summary>Gets or sets the teams.</summary>
    public List<TeamState> Teams { get; set; } = [];
    /// <summary>Gets or sets the dealer seat.</summary>
    public int? DealerSeat { get; set; }
    /// <summary>Gets or sets the turn.</summary>
    public TurnState Turn { get; set; } = new();
    /// <summary>Gets or sets the piles.</summary>
    public List<PileState> Piles { get; set; } = [];
    /// <summary>Gets or sets the auction.</summary>
    public AuctionState? Auction { get; set; }
    /// <summary>Gets or sets the contract.</summary>
    public ContractState? Contract { get; set; }
    /// <summary>Gets or sets the tricks.</summary>
    public TricksState? Tricks { get; set; }
    /// <summary>Gets or sets the exchange.</summary>
    public ExchangeState? Exchange { get; set; }
    /// <summary>Gets or sets the melds.</summary>
    public List<PinochleMeldState> Melds { get; set; } = [];
    /// <summary>Gets or sets the scores.</summary>
    public ScoreState Scores { get; set; } = new();
    /// <summary>Gets or sets the counters.</summary>
    public Dictionary<ScoreCounter, int> Counters { get; set; } = [];
    /// <summary>Gets or sets the flags.</summary>
    public Dictionary<GameFlag, bool> Flags { get; set; } = [];
    /// <summary>Gets or sets the variables.</summary>
    public Dictionary<string, JsonElement> Variables { get; set; } = new(StringComparer.Ordinal);
    /// <summary>Gets or sets the server pending decisions.</summary>
    public Dictionary<string, JsonElement> ServerPendingDecisions { get; set; } = new(StringComparer.Ordinal);
    /// <summary>Gets or sets the server knowledge.</summary>
    public ServerKnowledgeState ServerKnowledge { get; set; } = new();
    /// <summary>Gets or sets the server random.</summary>
    public ServerRandomState ServerRandom { get; set; } = new();
    /// <summary>Gets or sets the result.</summary>
    public GameResultState? Result { get; set; }
    /// <summary>Gets or sets the extensions.</summary>
    public Dictionary<string, JsonElement> Extensions { get; set; } = new(StringComparer.Ordinal);
}
