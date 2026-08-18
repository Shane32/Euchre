# Card Game Protocol Specifications

This directory contains language- and game-agnostic specifications for browsing,
joining, observing, and playing card games over one WebSocket connection.

- [Card Game WebSocket Protocol](card-game-websocket-protocol.md) defines a
  multiplexed, text-only transport based on the operation lifecycle of
  `graphql-transport-ws`. It also defines a small set of generic card-game
  messages carried by that transport.
- [Card Game State Management](game-state-management.md) defines authoritative
  state, player-specific views, hidden information, revisions, actions,
  subscriptions, recovery, and game-specific extension points.

The specifications deliberately do not define the rules of any particular
game. A client and server negotiate a game type and rules version, and both are
expected to understand that game's state document and legal actions.

## Layering

```text
WebSocket text frame
  -> multiplexing envelope (connection and operation lifecycle)
       -> payload
            -> compact card-game string or native JSON object
                 -> game-specific state or action data
```

The envelope is JSON because it follows the `graphql-transport-ws` shape. Most
payloads are compact strings. Messages containing structured or substantial
data may instead use a native JSON object as the envelope's `payload` value.

## C# service binding

The `GameService` implementation deliberately narrows the language-agnostic
wire model into strongly typed C# domain values:

- seats and teams are positive integer IDs; the four standard table positions
  are `TableSeats.North` (1) through `TableSeats.West` (4);
- game types, phases, action names, actor selectors, pile types, visibility
  policies, rule errors, meld names, flags, and score counters are enums;
- runtime pile references use `PileId`, with helpers such as
  `PileIds.Hand(TableSeats.North)`; and
- the few extensible JSON dictionary keys used by a game are centralized in
  constants such as `EuchreOptionNames` and `PinochleVariableNames`.

Strings remain only where the value is intentionally open-ended or external,
including game/player/action-instance IDs, display text, schema and rules
versions, card instance IDs, extension data, and handler names. The embedded
[euchre](../GameService/Games/Euchre/euchre.json) and
[pinochle](../GameService/Games/Pinochle/pinochle.json) definitions are the canonical
JSON examples for this C# binding.

### Project layout

```text
GameService/
├── Abstractions/       Public service contracts
├── Actions/            Requests, results, action definitions, and errors
├── Cards/              Card values, card state, and deck definitions
├── Definitions/        Game schema, phases, options, scoring, and loader
├── Games/
│   ├── Euchre/         Euchre service, constants, and embedded schema
│   └── Pinochle/       Pinochle service, meld types, constants, and schema
├── History/             Replayable command history and persistence models
├── Infrastructure/     Shared service engine and internal utilities
├── Piles/              Pile identities, state, ownership, and behavior
├── Players/            Players, teams, seats, and connection state
├── Serialization/      Custom JSON converters
└── State/              Shared runtime, auction, contract, turn, and score state
```

Each directory uses its matching `GameService.<Directory>` namespace. Game
implementations use `GameService.Games.<GameType>`.

### History and restoration

`IGameService.GetHistory()` returns the authoritative replay history together
with the game ID, players, options, rules version, creation time, and base
random seed GUID. The first entry is the initial `deal`; later entries contain
every accepted player action. Rejected actions and idempotent retries do not
create entries.

Every deal initializes a new `ReplayableRandom` instance from a GUID seed. The
seed is recorded on the history entry whose execution caused that deal,
including automatic redeals. A restored service consumes those recorded seeds
rather than deriving new ones. This keeps shuffles reproducible even if seed
derivation changes.

Histories can be serialized as JSON and restored with
`GameServiceFactory.Restore(history)`, `EuchreGameService.Restore(history)`, or
`PinochleGameService.Restore(history)`. Restoration creates a fresh service and
replays commands through the normal validation and mutation paths; it does not
deserialize a mutable `GameState` snapshot. Because histories contain private
actions and random seeds, servers must treat them as authoritative private
data rather than player projections.
