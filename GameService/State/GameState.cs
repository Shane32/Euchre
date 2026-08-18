using System.Text.Json;

namespace GameService.State;

public sealed class GameState
{
    public string SchemaVersion { get; set; } = "card-game-state/1.0";
    public required string GameId { get; set; }
    public GameType GameType { get; set; }
    public required string RulesVersion { get; set; }
    public Dictionary<string, JsonElement> Options { get; set; } = new(StringComparer.Ordinal);
    public long Revision { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public GameStatus Status { get; set; } = GameStatus.Waiting;
    public int RoundNumber { get; set; }
    public int DealNumber { get; set; }
    public PhaseState Phase { get; set; } = new() { Id = GamePhase.Waiting };
    public List<PlayerState> Players { get; set; } = [];
    public List<TeamState> Teams { get; set; } = [];
    public int? DealerSeat { get; set; }
    public TurnState Turn { get; set; } = new();
    public List<PileState> Piles { get; set; } = [];
    public AuctionState? Auction { get; set; }
    public ContractState? Contract { get; set; }
    public TricksState? Tricks { get; set; }
    public ExchangeState? Exchange { get; set; }
    public List<PinochleMeldState> Melds { get; set; } = [];
    public ScoreState Scores { get; set; } = new();
    public Dictionary<ScoreCounter, int> Counters { get; set; } = [];
    public Dictionary<GameFlag, bool> Flags { get; set; } = [];
    public Dictionary<string, JsonElement> Variables { get; set; } = new(StringComparer.Ordinal);
    public Dictionary<string, JsonElement> ServerPendingDecisions { get; set; } = new(StringComparer.Ordinal);
    public ServerKnowledgeState ServerKnowledge { get; set; } = new();
    public ServerRandomState ServerRandom { get; set; } = new();
    public GameResultState? Result { get; set; }
    public Dictionary<string, JsonElement> Extensions { get; set; } = new(StringComparer.Ordinal);
}
