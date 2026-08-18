# Card Game State Model

Status: Draft specification, version 1.0

## 1. Purpose

This specification defines a reusable data model for card games. It focuses on
the variables a server must track rather than prescribing how game code should
be written.

The model separates three documents:

1. **Game definition**: immutable rules metadata, card set, pile definitions,
   phases, actions, state variables, and scoring for one game type and rules
   version.
2. **Authoritative game state**: the server's complete state for one game,
   including every card and private decision.
3. **Viewer state**: a projection of authoritative state containing only what
   one client is allowed to know.

The definition describes data and names rule handlers. It does not attempt to
encode arbitrary game logic as JSON or define the user interface. Client and
server are assumed to implement the named handlers for the same `gameType` and
`rulesVersion`.

All examples use JSON. The normative model is language-agnostic.

## 2. Common conventions

- IDs are opaque strings and are unique within their stated scope.
- Array positions are zero-based unless a game definition says otherwise.
- A card occurs in exactly one pile at a time. Historical records reference the
  card but do not own another copy of it.
- `null` means a known absence. An omitted optional member means it does not
  apply or has not been recorded.
- Scores, revisions, indexes, and counters are integers.
- Timestamps are UTC strings in RFC 3339 format.
- Property names beginning with `server` are never included in ordinary player
  or spectator projections.
- A rules implementation may add fields beneath `variables`, `metadata`, and
  `extensions`. Extension names SHOULD be scoped by game type or vendor.

## 3. Game definition document

A game definition describes the shape and capabilities of a game. Its top-level
members are:

| Member | Required | Purpose |
| --- | --- | --- |
| `schemaVersion` | yes | Version of this generic definition format. |
| `gameType` | yes | Stable game family identifier, such as `euchre`. |
| `rulesVersion` | yes | Immutable version of the selected rules and state schema. |
| `displayName` | yes | Human-readable game name. |
| `playerConfiguration` | yes | Seats, player limits, teams, and turn order. |
| `options` | no | Configurable values fixed when a game is created. |
| `cardSet` | yes | Suits, ranks, copies, and construction of card instances. |
| `pileDefinitions` | yes | Piles instantiated for a game and their visibility. |
| `stateVariables` | yes | Typed game-specific variables and their defaults. |
| `phases` | yes | Phase IDs, actors, allowed actions, and transition handlers. |
| `actions` | yes | Generic and game-specific action arguments and handlers. |
| `cardRules` | no | Named handlers for effective suit, rank, comparison, and legality. |
| `scoring` | no | Declarative score table and/or scoring handler. |
| `extensions` | no | Versioned implementation-specific definition data. |

### 3.1 Player configuration

`playerConfiguration` defines:

| Member | Purpose |
| --- | --- |
| `minimumPlayers`, `maximumPlayers` | Bounds checked before starting. |
| `seats` | Stable seat IDs in table order. |
| `teams` | Optional fixed teams and their seat membership. |
| `turnOrder` | Ordered seat IDs or a handler that computes them. |
| `allowSpectators` | Whether spectator membership is supported. |
| `allowJoinInProgress` | Whether an empty seat may be filled after starting. |

Games with dynamic teams may omit fixed team membership and store current teams
in runtime state.

### 3.2 Card-set definition

`cardSet` defines the values from which card instances are created:

```json
{
  "copies": 1,
  "suits": [
    { "id": "clubs", "symbol": "C", "color": "black" },
    { "id": "diamonds", "symbol": "D", "color": "red" }
  ],
  "ranks": [
    { "id": "nine", "symbol": "9" },
    { "id": "ten", "symbol": "10" }
  ],
  "construction": {
    "kind": "cartesianProduct",
    "cardIdFormat": "{rank}.{suit}"
  }
}
```

`copies` creates distinct card instances with the same card value. Pinochle and
multi-deck solitaire therefore use `copies` greater than one. A nonstandard
deck may replace `cartesianProduct` with an explicit `cards` array.

The card set describes printed identity. Effective rank, suit, value, or wild
status may change with runtime state and is computed by handlers in `cardRules`.

### 3.3 Pile definitions

Every card-bearing location is a pile. Hands, stock, waste, kitty, widow,
tableau columns, foundations, the current trick, capture piles, meld displays,
pass selections, and discard piles all use the same representation.

A pile definition has:

| Member | Required | Purpose |
| --- | --- | --- |
| `id` | yes | Stable ID or template such as `hand:{seat}`. |
| `kind` | yes | Semantic category understood by game logic. |
| `instantiate` | yes | `once`, `perSeat`, or `perTeam`. |
| `owner` | no | `server`, `shared`, `seat:{seat}`, or `team:{team}`. |
| `ordered` | yes | Whether card order and position are meaningful. |
| `top` | conditional | `first` or `last`; required when an ordered pile has a top. |
| `select` | yes | `none`, `top`, `bottom`, `any`, or a rules handler. |
| `visibility` | yes | Who sees existence, count, identities, and order. |
| `initialization` | no | How cards initially enter the pile. |
| `metadata` | no | Game-defined immutable pile attributes. |

Visibility uses these standard audiences:

| Audience | Meaning |
| --- | --- |
| `none` | Only the authoritative server sees the value. |
| `public` | All players and permitted spectators see it. |
| `owner` | Only the owning seat or team sees it. |
| `team` | Members of the owning team see it. |
| `known` | A viewer sees card identities previously revealed to that viewer. |
| `handler:<id>` | A game-specific visibility handler decides. |

A typical private hand uses public count and `ownerOrKnown` card visibility:

```json
{
  "id": "hand:{seat}",
  "kind": "hand",
  "instantiate": "perSeat",
  "owner": "seat:{seat}",
  "ordered": false,
  "select": "any",
  "visibility": {
    "existence": "public",
    "count": "public",
    "cards": "handler:core.ownerOrKnown",
    "order": "owner"
  }
}
```

### 3.4 State-variable definitions

`stateVariables` declares game-specific values not naturally represented by
players, piles, turns, actions, or scores. Each variable has a `name`, `type`,
`default`, `visibility`, and optional `enum`, `nullable`, or validation bounds.

Portable types are:

- `boolean`, `integer`, `string`, and `timestamp`;
- `seatId`, `teamId`, `pileId`, `cardInstanceId`, `suitId`, and `rankId`;
- `array`, `map`, and `object` with a nested item or property schema; and
- `json` for a game-specific value validated by the rules module.

Examples include trump, dealer, lead seat, direction of passing, hearts broken,
spades broken, contract, maker, alone player, bags, stock recycle count, and
whether a declaration has occurred.

### 3.5 Phase definitions

Each phase has:

| Member | Purpose |
| --- | --- |
| `id` | Value stored in runtime `phase.id`. |
| `mode` | `automatic`, `exclusiveTurn`, or `simultaneous`. |
| `actors` | Selector such as `turn.activeSeat`, `allPlayers`, or a handler. |
| `allowedActions` | Action IDs accepted during the phase. |
| `entryHandler` | Optional handler run atomically on entry. |
| `completionHandler` | Handler that decides whether and where the phase advances. |
| `timeoutPolicy` | Optional timeout action or handler. |

The JSON definition names handlers; executable game logic implements them. This
keeps definitions portable without inventing an unsafe programming language in
the schema.

### 3.6 Action definitions

An action definition connects protocol input to game logic:

| Member | Purpose |
| --- | --- |
| `id` | Game action name. |
| `transport` | `playcard`, `other`, or `system`. |
| `phases` | Phases in which the action may occur. |
| `actor` | Actor selector or authorization handler. |
| `arguments` | Typed argument schema. |
| `validationHandler` | Checks legality without changing state. |
| `applyHandler` | Produces the atomic transition. |
| `visibility` | Who learns the action and its arguments. |

`arguments` uses the JSON Schema vocabulary for `type`, `required`,
`properties`, `items`, `enum`, `default`, and `additionalProperties`. This
specification additionally defines `enumFrom` as a path into the immutable game
definition whose IDs form the allowed values.

## 4. Runtime state variables

The authoritative state document contains the complete current game. The
following top-level containers cover common card-game requirements.

| Container | Required | Variables tracked |
| --- | --- | --- |
| Identity | yes | `schemaVersion`, `gameId`, `gameType`, `rulesVersion`, normalized `options` |
| Synchronization | yes | `revision`, `createdAt`, `updatedAt` |
| Lifecycle | yes | `status`, optional `result` |
| Participants | yes | `players`, `teams` |
| Progress | yes | `roundNumber`, `dealNumber`, `phase` |
| Seating and turn | yes | `dealerSeat`, `turn` |
| Cards | yes | `piles`, each containing `cards` |
| Auction | no | bids, passes, bidder, high bid, completion |
| Contract | no | declarer/maker, partner/team, suit, value, modifiers |
| Tricks | no | leader, led suit, plays, winner, completed tricks |
| Exchange | no | pass direction, required count, private selections, completion |
| Melds | no | declarations, cards, values, visibility |
| Scores | yes | current and cumulative player/team values |
| Timers | no | active deadline, per-seat time banks, pause state |
| Generic values | yes | `counters`, `flags`, and definition-declared `variables` |
| Private server data | no | `serverPendingDecisions`, `serverKnowledge`, `serverRandom` |
| History | no | event cursor, last action ID, retained revision range |

An implementation MAY omit an optional container when the game never uses it.
It SHOULD use the standard container when the concept applies rather than hiding
common data inside an opaque extension.

### 4.1 Authoritative state shape

This example shows the shape and variable names, not a particular game:

```json
{
  "schemaVersion": "card-game-state/1.0",
  "gameId": "game-42",
  "gameType": "example",
  "rulesVersion": "1.0",
  "options": {},
  "revision": 17,
  "createdAt": "2026-08-16T18:00:00Z",
  "updatedAt": "2026-08-16T18:02:31Z",
  "status": "active",
  "roundNumber": 1,
  "dealNumber": 1,
  "phase": {
    "id": "play",
    "step": 3,
    "startedAt": "2026-08-16T18:02:00Z"
  },
  "players": [],
  "teams": [],
  "dealerSeat": "west",
  "turn": {
    "mode": "exclusiveTurn",
    "activeSeats": ["north"],
    "prioritySeat": "north",
    "direction": "clockwise",
    "leadSeat": "east",
    "deadline": null
  },
  "piles": [],
  "auction": null,
  "contract": null,
  "tricks": null,
  "exchange": null,
  "melds": [],
  "scores": {
    "players": {},
    "teams": {},
    "target": null
  },
  "timers": null,
  "counters": {},
  "flags": {},
  "variables": {},
  "serverPendingDecisions": {},
  "serverKnowledge": {},
  "serverRandom": {},
  "history": {
    "eventCursor": "event-17",
    "lastActionId": "action-12",
    "oldestRetainedRevision": 0
  },
  "result": null,
  "extensions": {}
}
```

### 4.2 Players and teams

Each `players` item SHOULD contain:

```json
{
  "playerId": "player-1",
  "seat": "north",
  "displayName": "Ada",
  "teamId": "team-ns",
  "role": "player",
  "ready": true,
  "connection": "connected",
  "control": "human",
  "joinedAt": "2026-08-16T18:00:00Z",
  "metadata": {}
}
```

`connection` is normally `connected`, `disconnected`, or `replaced`. `control`
is normally `human`, `bot`, or `system`. Connection status does not determine
whether the seat still exists in game state.

Each team has `teamId`, `seatIds`, and optional display metadata. Scores belong
in `scores`, not in participant identity.

### 4.3 Piles and cards

The authoritative `piles` array is the source of truth for card location. Each
pile contains:

```json
{
  "pileId": "hand:north",
  "definitionId": "hand:{seat}",
  "kind": "hand",
  "owner": { "kind": "seat", "id": "north" },
  "cards": [
    {
      "instanceId": "card-01",
      "cardId": "ace.spades",
      "copy": 0,
      "rank": "ace",
      "suit": "spades",
      "faceUp": false,
      "orientation": "upright",
      "annotations": {}
    }
  ],
  "variables": {}
}
```

Required pile state is `pileId`, `definitionId`, `kind`, and `cards`. `owner` is
required when instantiated from a per-seat or per-team definition. Array order
is meaningful only when the definition's `ordered` value is true. For an
ordered pile, `top: first` makes `cards[0]` the top; `top: last` makes the final
array item the top.

Each authoritative card has a unique `instanceId`, a printed `cardId`, and a
`copy` number. `rank` and `suit` are repeated to make state self-contained and
may be omitted only when `cardId` can be resolved from the immutable definition.
`faceUp`, `orientation`, and `annotations` are placement state and may change
when the card moves. Effective suit or rank is normally derived rather than
stored.

A transition moves the card object from one pile's `cards` array to another.
The same `instanceId` MUST NOT occur twice or disappear unless the rules define
card creation/removal and record it explicitly.

### 4.4 Turn and phase

`phase.id` selects a phase from the game definition. `phase.step` is a
monotonic counter within the current phase and helps distinguish repeated turns.

`turn` tracks:

- `mode`: `automatic`, `exclusiveTurn`, `simultaneous`, or `realtime`;
- `activeSeats`: every seat currently permitted to act;
- `prioritySeat`: the seat whose action resolves first when applicable;
- `direction`: `clockwise`, `counterclockwise`, `fixed`, or game-defined;
- `leadSeat`: optional leader of the current trick or action cycle; and
- `deadline`: optional server deadline.

### 4.5 Auction and contract

Games with bidding use `auction`:

```json
{
  "round": 1,
  "activeSeat": "south",
  "calls": [
    { "seat": "north", "kind": "pass", "arguments": null, "revision": 5 },
    { "seat": "east", "kind": "bid", "arguments": { "value": 3 }, "revision": 6 }
  ],
  "highCall": { "seat": "east", "kind": "bid", "arguments": { "value": 3 } },
  "passedSeats": ["north"],
  "complete": false
}
```

When bidding finishes, its result is normalized into `contract`. Common members
are `kind`, `declarerSeat`, `teamId`, `value`, `suit`, `partnerSeat`,
`aloneSeat`, `doubling`, and game-specific `variables`. The auction remains as
history for the current deal.

### 4.6 Tricks

Trick-taking games use `tricks` while the cards themselves remain in piles:

```json
{
  "currentPileId": "current-trick",
  "leaderSeat": "north",
  "ledSuit": "clubs",
  "plays": [
    { "seat": "north", "cardInstanceId": "card-01", "order": 0 }
  ],
  "completed": [
    {
      "number": 0,
      "leaderSeat": "west",
      "winnerSeat": "south",
      "winningCardInstanceId": "card-12",
      "plays": []
    }
  ],
  "wonBySeat": { "north": 0, "east": 0, "south": 1, "west": 0 },
  "wonByTeam": { "team-ns": 1, "team-ew": 0 }
}
```

Completed trick records are references and facts. Cards won in those tricks are
located in the winner's or team's capture pile.

### 4.7 Exchanges and simultaneous choices

Hearts passing, partnership card exchange, discards, and similar mechanics use
`exchange`:

```json
{
  "kind": "pass",
  "direction": "left",
  "requiredCount": 3,
  "eligibleSeats": ["north", "east", "south", "west"],
  "submittedSeats": ["north", "south"],
  "complete": false,
  "publicRevealed": false
}
```

Actual unrevealed selections belong in `serverPendingDecisions`, keyed by seat.
They are copied into public or owner-visible state only when the rules reveal or
deliver them.

### 4.8 Melds, counters, flags, and variables

`melds` is an array of declarations with `seat`, optional `teamId`, `kind`,
`cardInstanceIds`, `value`, and `revealed`. This serves pinochle, rummy, and
similar games.

`counters` holds named integer values such as cards drawn, stock recycles, bags,
tricks attempted, or consecutive passes. `flags` holds booleans such as
`heartsBroken`, `spadesBroken`, `dealerStuck`, or `firstTrick`.

`variables` contains values declared by the definition's `stateVariables`.
Game logic MUST NOT place card location, player identity, current revision, or
other standard concepts in `variables` merely to avoid the standard containers.

### 4.9 Scores and result

`scores.players` and `scores.teams` are maps from ID to game-defined score
objects. A score object may contain `deal`, `round`, `total`, penalties, bonuses,
and game-specific counters. `scores.target` holds the configured win threshold
when one exists.

When `status` becomes `completed`, `result` records `winnerPlayerIds`,
`winnerTeamIds`, `reason`, `finalScores`, and `completedAt`. Ties use arrays with
multiple winners or an empty winner array plus a documented reason.

## 5. Viewer state and hidden cards

The server stores complete card objects. It derives a separate state projection
for each player or spectator according to pile visibility and recorded
knowledge. Clients MUST NOT receive full state and hide cards only in the UI.

For a visible position whose card is unknown, the projected pile contains an
unknown placeholder:

```json
{
  "unknown": true,
  "faceUp": false
}
```

For a known card, the projection contains a viewer-scoped reference and the
known printed identity:

```json
{
  "ref": "known-17",
  "cardId": "ace.spades",
  "rank": "ace",
  "suit": "spades",
  "faceUp": false
}
```

The server's `instanceId` and `copy` SHOULD NOT be exposed. This prevents IDs
from revealing duplicate-card identity or allowing correlation after a shuffle.

`serverKnowledge` tracks, per viewer, which card instances and partial facts are
known, any viewer-scoped references already assigned, and a `knowledgeEpoch`.
References may instead be derived when the projection is built. Knowledge is
gained from a player's hand, a face-up/public card, a revealed meld, a played
card, or another rules-defined reveal.

When a visible state variable or history record refers to a `cardInstanceId`,
the projection replaces it with the viewer-scoped `ref`. It omits or redacts the
reference when that card is unknown to the viewer.

A known card remains known while its movement is trackable. Turning it face down
does not make a player forget it. Shuffling it among indistinguishable unknown
cards, secretly reordering a collected pile, or another rules-defined
uncertainty event invalidates its viewer reference and advances the applicable
knowledge epoch.

Visibility rules apply independently to pile existence, count, card identity,
order, annotations, actions, pending decisions, and history. Errors and hashes
must also be computed from the viewer's authorized information.

## 6. State transitions and synchronization

An accepted action produces one atomic state transition:

1. authorize the actor and parse the action;
2. reject a duplicate action ID with different content, or return the recorded
   result for an equivalent retry;
3. compare the expected revision;
4. validate the action with the definition's handler;
5. move cards and update all affected standard containers and variables;
6. update viewer knowledge;
7. increment `revision` exactly once and persist the action result; and
8. publish each viewer's new projection.

Rejected actions do not change the revision. Concurrent actions are serialized
or rejected according to the current phase's mode. A server-generated deal,
timeout, bot move, or scoring step uses the same transition rules.

CGWS returns a complete projected state in the JSON `gamestate` payload. A delta
identifies `baseRevision` and `revision` and updates the same model. A client
that misses a base revision or computes a different projection hash requests a
fresh state.

The recommended projection hash is `jcssha256`: canonicalize the projected
`state` JSON with JCS (RFC 8785), hash its UTF-8 bytes with SHA-256, and encode
the result as unpadded base64url. The server never publishes a hash of hidden
authoritative state.

## 7. Complete euchre game definition example

This definition covers four-player partnership euchre with a 24-card deck,
clockwise play, optional stick-the-dealer, optional Ankle, and a target score.
Ankle lets only the final second-round bidder choose a suit, choose no trump, or
pass for a redeal. In a no-trump contract, jacks retain their printed suits and
ordinary ranks. If both Ankle and stick-the-dealer are enabled, Ankle takes
precedence so the final bidder is not forced to bid. Handler IDs name logic
shared by client and server; their implementations contain detailed move legality
and are not UI definitions.

```json
{
  "schemaVersion": "card-game-definition/1.0",
  "gameType": "euchre",
  "rulesVersion": "standard-24/1.2",
  "displayName": "Euchre",
  "playerConfiguration": {
    "minimumPlayers": 4,
    "maximumPlayers": 4,
    "seats": ["north", "east", "south", "west"],
    "teams": [
      { "teamId": "team-ns", "seatIds": ["north", "south"] },
      { "teamId": "team-ew", "seatIds": ["east", "west"] }
    ],
    "turnOrder": ["north", "east", "south", "west"],
    "allowSpectators": true,
    "allowJoinInProgress": false
  },
  "options": {
    "targetScore": {
      "type": "integer",
      "default": 10,
      "minimum": 1,
      "maximum": 21
    },
    "stickTheDealer": {
      "type": "boolean",
      "default": false
    },
    "ankle": {
      "type": "boolean",
      "default": false
    },
    "aceNoFaceRedeal": {
      "type": "boolean",
      "default": false
    }
  },
  "cardSet": {
    "copies": 1,
    "suits": [
      { "id": "clubs", "symbol": "C", "color": "black" },
      { "id": "diamonds", "symbol": "D", "color": "red" },
      { "id": "hearts", "symbol": "H", "color": "red" },
      { "id": "spades", "symbol": "S", "color": "black" }
    ],
    "ranks": [
      { "id": "nine", "symbol": "9", "baseOrder": 0 },
      { "id": "ten", "symbol": "10", "baseOrder": 1 },
      { "id": "jack", "symbol": "J", "baseOrder": 2 },
      { "id": "queen", "symbol": "Q", "baseOrder": 3 },
      { "id": "king", "symbol": "K", "baseOrder": 4 },
      { "id": "ace", "symbol": "A", "baseOrder": 5 }
    ],
    "construction": {
      "kind": "cartesianProduct",
      "cardIdFormat": "{rank}.{suit}"
    }
  },
  "pileDefinitions": [
    {
      "id": "deck",
      "kind": "stock",
      "instantiate": "once",
      "owner": "server",
      "ordered": true,
      "top": "first",
      "select": "top",
      "visibility": {
        "existence": "public",
        "count": "none",
        "cards": "none",
        "order": "none"
      },
      "initialization": {
        "cards": "all",
        "shuffle": "handler:core.secureShuffle"
      }
    },
    {
      "id": "hand:{seat}",
      "kind": "hand",
      "instantiate": "perSeat",
      "owner": "seat:{seat}",
      "ordered": false,
      "select": "any",
      "visibility": {
        "existence": "public",
        "count": "public",
        "cards": "handler:core.ownerOrKnown",
        "order": "owner"
      }
    },
    {
      "id": "kitty",
      "kind": "kitty",
      "instantiate": "once",
      "owner": "server",
      "ordered": true,
      "top": "first",
      "select": "handler:euchre.kittySelection",
      "visibility": {
        "existence": "public",
        "count": "public",
        "cards": "handler:core.faceUpOrKnown",
        "order": "handler:core.visibleCardPositions"
      }
    },
    {
      "id": "current-trick",
      "kind": "trick",
      "instantiate": "once",
      "owner": "shared",
      "ordered": true,
      "top": "last",
      "select": "none",
      "visibility": {
        "existence": "public",
        "count": "public",
        "cards": "public",
        "order": "public"
      }
    },
    {
      "id": "won:{team}",
      "kind": "capture",
      "instantiate": "perTeam",
      "owner": "team:{team}",
      "ordered": false,
      "select": "none",
      "visibility": {
        "existence": "public",
        "count": "public",
        "cards": "known",
        "order": "none"
      }
    }
  ],
  "stateVariables": [
    { "name": "upcardInstanceId", "type": "cardInstanceId", "default": null, "nullable": true, "visibility": "known" },
    { "name": "biddingRound", "type": "integer", "default": 0, "minimum": 0, "maximum": 2, "visibility": "public" },
    { "name": "makerSeat", "type": "seatId", "default": null, "nullable": true, "visibility": "public" },
    { "name": "trump", "type": "suitId", "default": null, "nullable": true, "visibility": "public" },
    { "name": "noTrump", "type": "boolean", "default": false, "visibility": "public" },
    { "name": "aloneSeat", "type": "seatId", "default": null, "nullable": true, "visibility": "public" },
    { "name": "inactiveSeat", "type": "seatId", "default": null, "nullable": true, "visibility": "public" },
    { "name": "orderedUp", "type": "boolean", "default": false, "visibility": "public" },
    { "name": "tricksPlayed", "type": "integer", "default": 0, "minimum": 0, "maximum": 5, "visibility": "public" }
  ],
  "phases": [
    {
      "id": "waiting",
      "mode": "simultaneous",
      "actors": "allPlayers",
      "allowedActions": ["ready"],
      "completionHandler": "handler:euchre.allPlayersReady"
    },
    {
      "id": "deal",
      "mode": "automatic",
      "actors": "system",
      "allowedActions": ["deal"],
      "completionHandler": "handler:euchre.beginFirstBiddingRound"
    },
    {
      "id": "bid-round-one",
      "mode": "exclusiveTurn",
      "actors": "turn.activeSeat",
      "allowedActions": ["pass", "orderup"],
      "completionHandler": "handler:euchre.advanceFirstBiddingRound"
    },
    {
      "id": "dealer-discard",
      "mode": "exclusiveTurn",
      "actors": "dealerSeat",
      "allowedActions": ["discard"],
      "completionHandler": "handler:euchre.beginPlayAfterDiscard"
    },
    {
      "id": "bid-round-two",
      "mode": "exclusiveTurn",
      "actors": "turn.activeSeat",
      "allowedActions": ["pass", "choosetrump", "choosenotrump"],
      "completionHandler": "handler:euchre.advanceSecondBiddingRound"
    },
    {
      "id": "play",
      "mode": "exclusiveTurn",
      "actors": "turn.activeSeat",
      "allowedActions": ["playcard"],
      "completionHandler": "handler:euchre.advanceTrickOrScore"
    },
    {
      "id": "score-deal",
      "mode": "automatic",
      "actors": "system",
      "allowedActions": ["scoredeal"],
      "completionHandler": "handler:euchre.nextDealOrComplete"
    },
    {
      "id": "completed",
      "mode": "automatic",
      "actors": "none",
      "allowedActions": [],
      "completionHandler": "handler:core.never"
    }
  ],
  "actions": [
    {
      "id": "ready",
      "transport": "other",
      "phases": ["waiting"],
      "actor": "self",
      "arguments": { "type": "object", "properties": {}, "additionalProperties": false },
      "validationHandler": "handler:core.seatedPlayer",
      "applyHandler": "handler:core.markReady",
      "visibility": "public"
    },
    {
      "id": "deal",
      "transport": "system",
      "phases": ["deal"],
      "actor": "system",
      "arguments": { "type": "object", "properties": {}, "additionalProperties": false },
      "validationHandler": "handler:core.systemOnly",
      "applyHandler": "handler:euchre.dealFiveAndTurnUp",
      "visibility": "handler:euchre.dealVisibility"
    },
    {
      "id": "pass",
      "transport": "other",
      "phases": ["bid-round-one", "bid-round-two"],
      "actor": "turn.activeSeat",
      "arguments": { "type": "object", "properties": {}, "additionalProperties": false },
      "validationHandler": "handler:euchre.canPass",
      "applyHandler": "handler:euchre.applyPass",
      "visibility": "public"
    },
    {
      "id": "orderup",
      "transport": "other",
      "phases": ["bid-round-one"],
      "actor": "turn.activeSeat",
      "arguments": {
        "type": "object",
        "properties": { "alone": { "type": "boolean", "default": false } },
        "additionalProperties": false
      },
      "validationHandler": "handler:euchre.canOrderUp",
      "applyHandler": "handler:euchre.applyOrderUp",
      "visibility": "public"
    },
    {
      "id": "choosetrump",
      "transport": "other",
      "phases": ["bid-round-two"],
      "actor": "turn.activeSeat",
      "arguments": {
        "type": "object",
        "required": ["suit"],
        "properties": {
          "suit": { "type": "string", "enumFrom": "cardSet.suits" },
          "alone": { "type": "boolean", "default": false }
        },
        "additionalProperties": false
      },
      "validationHandler": "handler:euchre.canChooseTrump",
      "applyHandler": "handler:euchre.applyChooseTrump",
      "visibility": "public"
    },
    {
      "id": "choosenotrump",
      "transport": "other",
      "phases": ["bid-round-two"],
      "actor": "turn.activeSeat",
      "arguments": {
        "type": "object",
        "properties": {
          "alone": { "type": "boolean", "default": false }
        },
        "additionalProperties": false
      },
      "validationHandler": "handler:euchre.canChooseNoTrump",
      "applyHandler": "handler:euchre.applyChooseNoTrump",
      "visibility": "public"
    },
    {
      "id": "discard",
      "transport": "playcard",
      "phases": ["dealer-discard"],
      "actor": "dealerSeat",
      "arguments": {
        "sourcePile": "hand:{dealerSeat}",
        "sourceCard": "required",
        "targetPile": "kitty"
      },
      "validationHandler": "handler:euchre.canDealerDiscard",
      "applyHandler": "handler:euchre.applyDealerDiscard",
      "visibility": "handler:euchre.discardVisibility"
    },
    {
      "id": "playcard",
      "transport": "playcard",
      "phases": ["play"],
      "actor": "turn.activeSeat",
      "arguments": {
        "sourcePile": "hand:{actorSeat}",
        "sourceCard": "required",
        "targetPile": "current-trick"
      },
      "validationHandler": "handler:euchre.canPlayCard",
      "applyHandler": "handler:euchre.applyPlayCardAndResolveTrick",
      "visibility": "public"
    },
    {
      "id": "scoredeal",
      "transport": "system",
      "phases": ["score-deal"],
      "actor": "system",
      "arguments": { "type": "object", "properties": {}, "additionalProperties": false },
      "validationHandler": "handler:core.systemOnly",
      "applyHandler": "handler:euchre.scoreDeal",
      "visibility": "public"
    }
  ],
  "cardRules": {
    "effectiveSuitHandler": "handler:euchre.effectiveSuitWithLeftBower",
    "followSuitHandler": "handler:euchre.mustFollowEffectiveSuit",
    "trickWinnerHandler": "handler:euchre.trickWinner",
    "trumpOrder": ["right-bower", "left-bower", "ace", "king", "queen", "ten", "nine"],
    "plainSuitOrder": ["ace", "king", "queen", "jack", "ten", "nine"]
  },
  "scoring": {
    "scope": "team",
    "targetOption": "targetScore",
    "outcomes": [
      { "condition": "makers.notAlone.win3or4", "makerPoints": 1, "defenderPoints": 0 },
      { "condition": "makers.notAlone.win5", "makerPoints": 2, "defenderPoints": 0 },
      { "condition": "aloneMaker.win3or4", "makerPoints": 1, "defenderPoints": 0 },
      { "condition": "aloneMaker.win5", "makerPoints": 4, "defenderPoints": 0 },
      { "condition": "makers.win0to2", "makerPoints": 0, "defenderPoints": 2 }
    ],
    "handler": "handler:euchre.scoreDeal"
  },
  "extensions": {
    "euchre": {
      "dealPattern": [[3, 2], [2, 3]],
      "firstBidder": "leftOfDealer",
      "secondRoundForbiddenSuit": "upcardSuit",
      "ankle": "dealer may choose a suit, choose no trump, or pass after the other second-round bidders pass",
      "aceNoFaceDefinition": "exactly one ace and no jacks, queens, or kings",
      "redealWhenAllPass": true
    }
  }
}
```

## 8. Complete authoritative euchre deal example

This runtime document includes all required containers and all 24 card
instances; it omits only empty optional containers. It is authoritative server
state during the first bidding round. A client projection would replace cards
in other players' hands with unknown placeholders.

```json
{
  "schemaVersion": "card-game-state/1.0",
  "gameId": "euchre-42",
  "gameType": "euchre",
  "rulesVersion": "standard-24/1.2",
  "revision": 4,
  "createdAt": "2026-08-16T18:00:00Z",
  "updatedAt": "2026-08-16T18:01:00Z",
  "status": "active",
  "roundNumber": 1,
  "dealNumber": 1,
  "phase": { "id": "bid-round-one", "step": 0 },
  "players": [
    { "playerId": "p1", "seat": "north", "teamId": "team-ns", "role": "player", "ready": true, "connection": "connected", "control": "human" },
    { "playerId": "p2", "seat": "east", "teamId": "team-ew", "role": "player", "ready": true, "connection": "connected", "control": "human" },
    { "playerId": "p3", "seat": "south", "teamId": "team-ns", "role": "player", "ready": true, "connection": "connected", "control": "human" },
    { "playerId": "p4", "seat": "west", "teamId": "team-ew", "role": "player", "ready": true, "connection": "connected", "control": "human" }
  ],
  "teams": [
    { "teamId": "team-ns", "seatIds": ["north", "south"] },
    { "teamId": "team-ew", "seatIds": ["east", "west"] }
  ],
  "dealerSeat": "west",
  "turn": {
    "mode": "exclusiveTurn",
    "activeSeats": ["north"],
    "prioritySeat": "north",
    "direction": "clockwise",
    "leadSeat": null,
    "deadline": null
  },
  "piles": [
    { "pileId": "deck", "definitionId": "deck", "kind": "stock", "owner": { "kind": "server", "id": "server" }, "cards": [] },
    {
      "pileId": "hand:north",
      "definitionId": "hand:{seat}",
      "kind": "hand",
      "owner": { "kind": "seat", "id": "north" },
      "cards": [
        { "instanceId": "c01", "cardId": "nine.clubs", "copy": 0, "rank": "nine", "suit": "clubs", "faceUp": false },
        { "instanceId": "c02", "cardId": "jack.clubs", "copy": 0, "rank": "jack", "suit": "clubs", "faceUp": false },
        { "instanceId": "c03", "cardId": "ace.diamonds", "copy": 0, "rank": "ace", "suit": "diamonds", "faceUp": false },
        { "instanceId": "c04", "cardId": "queen.hearts", "copy": 0, "rank": "queen", "suit": "hearts", "faceUp": false },
        { "instanceId": "c05", "cardId": "king.spades", "copy": 0, "rank": "king", "suit": "spades", "faceUp": false }
      ]
    },
    {
      "pileId": "hand:east",
      "definitionId": "hand:{seat}",
      "kind": "hand",
      "owner": { "kind": "seat", "id": "east" },
      "cards": [
        { "instanceId": "c06", "cardId": "ten.clubs", "copy": 0, "rank": "ten", "suit": "clubs", "faceUp": false },
        { "instanceId": "c07", "cardId": "queen.clubs", "copy": 0, "rank": "queen", "suit": "clubs", "faceUp": false },
        { "instanceId": "c08", "cardId": "nine.diamonds", "copy": 0, "rank": "nine", "suit": "diamonds", "faceUp": false },
        { "instanceId": "c09", "cardId": "jack.hearts", "copy": 0, "rank": "jack", "suit": "hearts", "faceUp": false },
        { "instanceId": "c10", "cardId": "ace.spades", "copy": 0, "rank": "ace", "suit": "spades", "faceUp": false }
      ]
    },
    {
      "pileId": "hand:south",
      "definitionId": "hand:{seat}",
      "kind": "hand",
      "owner": { "kind": "seat", "id": "south" },
      "cards": [
        { "instanceId": "c11", "cardId": "king.clubs", "copy": 0, "rank": "king", "suit": "clubs", "faceUp": false },
        { "instanceId": "c12", "cardId": "ten.diamonds", "copy": 0, "rank": "ten", "suit": "diamonds", "faceUp": false },
        { "instanceId": "c13", "cardId": "queen.diamonds", "copy": 0, "rank": "queen", "suit": "diamonds", "faceUp": false },
        { "instanceId": "c14", "cardId": "king.hearts", "copy": 0, "rank": "king", "suit": "hearts", "faceUp": false },
        { "instanceId": "c15", "cardId": "nine.spades", "copy": 0, "rank": "nine", "suit": "spades", "faceUp": false }
      ]
    },
    {
      "pileId": "hand:west",
      "definitionId": "hand:{seat}",
      "kind": "hand",
      "owner": { "kind": "seat", "id": "west" },
      "cards": [
        { "instanceId": "c16", "cardId": "ace.clubs", "copy": 0, "rank": "ace", "suit": "clubs", "faceUp": false },
        { "instanceId": "c17", "cardId": "jack.diamonds", "copy": 0, "rank": "jack", "suit": "diamonds", "faceUp": false },
        { "instanceId": "c18", "cardId": "king.diamonds", "copy": 0, "rank": "king", "suit": "diamonds", "faceUp": false },
        { "instanceId": "c19", "cardId": "ace.hearts", "copy": 0, "rank": "ace", "suit": "hearts", "faceUp": false },
        { "instanceId": "c20", "cardId": "ten.spades", "copy": 0, "rank": "ten", "suit": "spades", "faceUp": false }
      ]
    },
    {
      "pileId": "kitty",
      "definitionId": "kitty",
      "kind": "kitty",
      "owner": { "kind": "server", "id": "server" },
      "cards": [
        { "instanceId": "c21", "cardId": "nine.hearts", "copy": 0, "rank": "nine", "suit": "hearts", "faceUp": true },
        { "instanceId": "c22", "cardId": "ten.hearts", "copy": 0, "rank": "ten", "suit": "hearts", "faceUp": false },
        { "instanceId": "c23", "cardId": "jack.spades", "copy": 0, "rank": "jack", "suit": "spades", "faceUp": false },
        { "instanceId": "c24", "cardId": "queen.spades", "copy": 0, "rank": "queen", "suit": "spades", "faceUp": false }
      ]
    },
    { "pileId": "current-trick", "definitionId": "current-trick", "kind": "trick", "owner": { "kind": "shared", "id": "shared" }, "cards": [] },
    { "pileId": "won:team-ns", "definitionId": "won:{team}", "kind": "capture", "owner": { "kind": "team", "id": "team-ns" }, "cards": [] },
    { "pileId": "won:team-ew", "definitionId": "won:{team}", "kind": "capture", "owner": { "kind": "team", "id": "team-ew" }, "cards": [] }
  ],
  "auction": {
    "round": 1,
    "activeSeat": "north",
    "calls": [],
    "highCall": null,
    "passedSeats": [],
    "complete": false
  },
  "contract": null,
  "tricks": {
    "currentPileId": "current-trick",
    "leaderSeat": null,
    "ledSuit": null,
    "plays": [],
    "completed": [],
    "wonBySeat": { "north": 0, "east": 0, "south": 0, "west": 0 },
    "wonByTeam": { "team-ns": 0, "team-ew": 0 }
  },
  "melds": [],
  "scores": {
    "players": {},
    "teams": {
      "team-ns": { "deal": 0, "total": 0 },
      "team-ew": { "deal": 0, "total": 0 }
    },
    "target": 10
  },
  "counters": {},
  "flags": { "firstTrick": true },
  "variables": {
    "upcardInstanceId": "c21",
    "biddingRound": 1,
    "makerSeat": null,
    "trump": null,
    "noTrump": false,
    "aloneSeat": null,
    "inactiveSeat": null,
    "orderedUp": false,
    "tricksPlayed": 0
  },
  "serverPendingDecisions": {},
  "serverKnowledge": {
    "epoch": 1,
    "viewers": {
      "p1": { "knownCardInstanceIds": ["c01", "c02", "c03", "c04", "c05", "c21"] },
      "p2": { "knownCardInstanceIds": ["c06", "c07", "c08", "c09", "c10", "c21"] },
      "p3": { "knownCardInstanceIds": ["c11", "c12", "c13", "c14", "c15", "c21"] },
      "p4": { "knownCardInstanceIds": ["c16", "c17", "c18", "c19", "c20", "c21"] }
    }
  },
  "serverRandom": {
    "shuffleId": "shuffle-1",
    "generator": "system-csprng",
    "auditReference": "audit:euchre-42:deal-1"
  },
  "result": null,
  "extensions": {}
}
```

## 9. Coverage of other common games

The same variables cover the requested game families without changing the core
model:

| Game | Piles | Additional standard state |
| --- | --- | --- |
| Double solitaire | Per-player stock, waste, hand/tableau; shared foundations | `turn.mode: realtime`, stock recycle counters, shared-pile conflict handling |
| Pinochle | Hands, kitty/widow where used, meld areas, current trick, team capture piles | Multi-copy card instances, `auction`, `contract`, `melds`, team scores |
| Spades | Hands, current trick, team capture piles | Bids in `auction`, nil flags in `contract.variables`, `spadesBroken`, bags and team scores |
| Hearts | Hands, pass-selection piles or private pending selections, current trick, capture piles | `exchange`, `heartsBroken`, pass direction, penalty totals, shoot-the-moon scoring |
| Euchre | Hands, kitty, current trick, team capture piles | Two-round auction, maker/trump/no-trump/alone variables, Ankle and stick-the-dealer options, effective-suit handler, team scores |

Game-specific definitions add phases, actions, variables, and handlers while
retaining the same players, teams, piles, cards, revisions, knowledge, and score
containers.

## 10. Definition and state validation

A conforming implementation validates at least these invariants:

1. `gameType` and `rulesVersion` match one immutable game definition.
2. Every instantiated pile matches one pile definition and has the required
   owner.
3. Every card instance is permitted by `cardSet` and appears in exactly one pile.
4. Players occupy valid unique seats and fixed team membership matches the
   definition.
5. The current phase and action exist in the definition, and the actor matches
   the phase and action selectors.
6. Every declared state variable has the correct type and visibility.
7. References in turns, auctions, contracts, tricks, melds, and pending choices
   resolve to existing seats, teams, piles, or card instances.
8. Viewer projections obey pile and variable visibility and preserve only valid
   prior knowledge.
9. Revisions increase atomically and action IDs are idempotent.
10. A projected JSON state produces the advertised viewer-specific hash.
