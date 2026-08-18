# Card Game WebSocket Protocol

Status: Draft specification, version 1.0

## 1. Purpose and scope

The Card Game WebSocket Protocol (CGWS) allows a client to browse games, join or
observe a game, submit actions, exchange messages, and subscribe to state while
sharing one bidirectional WebSocket connection. Independent requests and
long-lived streams are multiplexed by operation identifier.

CGWS adopts the connection and operation lifecycle of the GraphQL over
WebSocket `graphql-transport-ws` protocol, but does not carry GraphQL. Most
application payloads are compact strings. A payload MAY instead be a native JSON
object when its message definition calls for structured or substantial data.

This document defines two layers:

1. a text-only multiplexing envelope; and
2. a default, generic vocabulary with compact text and structured JSON forms.

The state schema, rules, legal moves, and game-specific meanings are selected by
`gameType` and `rulesVersion` and are outside this transport specification. See
[Card Game State Management](game-state-management.md).

The key words MUST, MUST NOT, REQUIRED, SHOULD, SHOULD NOT, and MAY are to be
interpreted as normative requirements.

## 2. WebSocket negotiation

- The WebSocket subprotocol name is `card-game-transport-ws`.
- Every protocol message MUST occupy one complete WebSocket text message. A
  WebSocket implementation MAY fragment that message into frames.
- Binary WebSocket messages MUST NOT be used and receipt of one SHOULD close the
  connection with code `4400`.
- Each WebSocket text message MUST contain one UTF-8 JSON envelope object.
- Implementations SHOULD enforce configurable limits for envelope size, payload
  size, active operations, and message rate.

Although its lifecycle is intentionally familiar to
`graphql-transport-ws` implementations, CGWS uses a distinct subprotocol name so
that a GraphQL server cannot accidentally interpret CGWS payloads as GraphQL
operations.

## 3. Envelope

An envelope has these members:

| Member | Type | Meaning |
| --- | --- | --- |
| `type` | string | REQUIRED message type from section 4. |
| `id` | string | REQUIRED for operation messages; absent for connection messages. |
| `payload` | string or object | Optional compact or structured application data. |

Unknown envelope members SHOULD be ignored for forward compatibility. An
unknown `type`, an incorrectly present or absent `id`, or a payload type not
allowed for that message is a protocol error. Arrays, numbers, booleans, and
null MUST NOT be used as complete payload values.

The following is a real wire message:

```json
{"id":"op-17","type":"subscribe","payload":"getgamestate-42"}
```

The receiver parses the envelope, then interprets the resulting
`getgamestate-42` string according to section 7.

## 4. Envelope message types

| Type | Direction | `id` | `payload` | Purpose |
| --- | --- | --- | --- | --- |
| `connection_init` | client to server | no | required object | Initialize the logical connection with a protocol version and authorization value. |
| `connection_ack` | server to client | no | required object | Accept initialization and confirm the protocol version. |
| `ping` | either | no | optional string or object | Request a prompt `pong`; may carry opaque diagnostic data. |
| `pong` | either | no | optional string or object | Reply to `ping`, normally echoing its payload exactly. |
| `subscribe` | client to server | yes | required string or object | Start a request, action, or subscription operation. |
| `next` | server to client | yes | required string or object | Deliver one non-terminal result on an operation. |
| `error` | server to client | yes | required string or object | Terminate an operation with an error. |
| `complete` | either | yes | server: optional string or object; client: absent | Server: optionally deliver one final result, then end. Client: cancel. |

`subscribe` retains the GraphQL protocol's name even when the operation is a
one-shot request or action. This keeps the outer state machine small: the inner
message determines whether an operation is unary or streaming.

Payload objects use the schemas defined here; they are not GraphQL operations or
GraphQL's array-valued error format. Implementations SHOULD prefer compact
strings when the message has only a few scalar values.

## 5. Connection lifecycle

1. After the WebSocket opens, the client MUST send exactly one
   `connection_init`, with the payload defined below, within the server's
   initialization timeout.
2. The server validates the initialization payload. It sends
   `connection_ack` on success. It MAY close with `4401` or `4403` if
   authentication is missing or denied.
3. The client MUST NOT send `subscribe` before `connection_ack`. A server that
   receives one early SHOULD close with `4401`.
4. After acknowledgement, either peer MAY send `ping` at any time. The other
   peer SHOULD answer promptly with `pong`. An unsolicited `pong` MAY be used as
   a heartbeat.
5. Either peer may close the WebSocket normally with code `1000`.

### 5.1 Initialization payload

The `connection_init` payload MUST be a JSON object with these members:

| Member | Type | Requirement | Meaning |
| --- | --- | --- | --- |
| `version` | string | REQUIRED | CGWS application protocol version; currently `1.0`. |
| `authorization` | string | REQUIRED | Authorization value using the syntax of an HTTP `Authorization` field. |
| `resumeToken` | string | optional | Opaque token from an earlier acknowledged session. |

The payload is:

```json
{
  "version": "1.0",
  "authorization": "Bearer opaque-access-token"
}
```

The complete wire envelope contains the object directly:

```json
{"type":"connection_init","payload":{"version":"1.0","authorization":"Bearer opaque-access-token"}}
```

The `authorization` value consists of a non-empty authentication scheme,
optionally followed by one or more spaces and scheme-specific credentials. The
scheme is case-insensitive; its credential text is opaque to CGWS and MUST NOT
contain control characters. For example, OAuth access tokens normally use the
`Bearer` scheme. An implementation that permits unauthenticated access MUST use
an explicit documented scheme such as `Anonymous`; `authorization` is still
required.

Unknown object members SHOULD be ignored for forward compatibility. Missing or
incorrectly typed required members, arrays or non-object JSON values, duplicate
member names, invalid authorization syntax, and unsupported versions are
initialization errors. Authentication credentials MUST be protected by TLS in
production, SHOULD be short-lived, and MUST NOT appear in logs or error text.

### 5.2 Acknowledgement payload

The `connection_ack` payload MUST be a JSON object. It confirms the version
and may return session and limit information:

```json
{
  "version": "1.0",
  "sessionId": "session-7",
  "resumeToken": "opaque-resume-token",
  "limits": {
    "maxOperations": 32,
    "maxPayloadBytes": 65536
  }
}
```

`version` is the only required member and MUST equal the supported version from
`connection_init`. `sessionId` and `resumeToken` are opaque strings. Members of
`limits` are positive integers; defined members are `maxOperations` and
`maxPayloadBytes`. Unknown members SHOULD be ignored, and duplicate member names
are invalid.

## 6. Operation lifecycle and multiplexing

An operation identifier is an opaque, non-empty string selected by the client.
It MUST be unique among operations active on that connection. It has no meaning
outside the connection and MAY be reused only after the prior operation has
terminated.

```text
client                         server
   |--- subscribe (id=A) -------->|
   |--- subscribe (id=B) -------->|
   |<-- next (id=B) --------------|
   |<-- next (id=A) --------------|
   |<-- complete (id=B) ----------|
   |<-- next (id=A) --------------|
   |--- complete (id=A) --------->|  cancellation
```

- A unary operation produces either one `next` followed by an empty `complete`,
  or one server `complete` carrying the result payload.
- A streaming operation produces zero or more `next` messages until either
  peer sends `complete` or the server sends `error`.
- Results for different identifiers MAY interleave. Results for one identifier
  MUST be sent in causal order.
- `error` and server-sent `complete` are terminal. When a server-sent `complete`
  has a payload, the client MUST process that payload exactly as if it had
  arrived in a `next` envelope immediately before the `complete`, and then mark
  the operation complete. This final value participates in normal ordering and
  validation. The operation is terminal even if its final payload is invalid or
  cannot be processed.
- A peer MUST ignore later operation messages for a terminal identifier, except
  that an implementation MAY treat repeated terminal messages as a protocol
  error.
- A client cancels an active operation by sending `complete`. The server MUST
  send no payload on that envelope. The server MUST stop producing results for
  it as soon as practical and MUST NOT reply with a second `complete` solely to
  acknowledge cancellation.
- A server receiving a duplicate active identifier SHOULD close with `4409`.
- Connection loss terminates all operation identifiers. Application-level
  action idempotency and subscription recovery are covered in sections 8 and 9;
  reconnecting does not make an old operation identifier active again.

For example, this one server message is semantically identical to a `next` with
the same payload followed immediately by an empty `complete`:

```json
{"id":"op-17","type":"complete","payload":"stateunchanged-42-124-abc123"}
```

## 7. Card-game payload formats

The application protocol is `card-game-v1`. It has two payload forms:

- **Compact text** is the default for common requests and small results. It
  avoids JSON field names and punctuation.
- **JSON object** is used when a message is naturally structured or holds
  substantial data. It MAY also represent any compact message when the sender
  prefers named fields over size efficiency.

The JSON type of the envelope's `payload` member selects the form: a string is
compact text and an object is structured JSON. There is no content sniffing and
a string containing serialized JSON is not a JSON-object payload. Arrays,
numbers, booleans, and null are not complete payloads, although they may appear
as member values inside an object.

`connection_init` and `connection_ack` always use their JSON schemas in section
5. `gamestate` always uses JSON as defined in section 9. Other operation
messages MAY use either form unless their definition says otherwise. Senders
SHOULD use compact text for simple, frequent messages.

### 7.1 Compact text

The compact form is parsed as follows:

```abnf
message       = verb *( "-" component )
verb          = 1*( %x61-7A / DIGIT )
component     = *( safe / pct-encoded )
safe          = ALPHA / DIGIT / "." / "_" / "~"
pct-encoded   = "%" HEXDIG HEXDIG
```

The first component is a lowercase verb. Later components are positional and
separated by literal hyphens. Within a component, UTF-8 bytes outside `safe`,
including literal hyphen, percent, spaces, and non-ASCII text, MUST be encoded
as `%HH`. Senders MUST use uppercase hexadecimal; receivers MUST accept either
case. Senders leave `safe` bytes unescaped. Receivers percent-decode to bytes
only after splitting on literal hyphens, and then require valid UTF-8. CGWS does
not otherwise normalize Unicode. For example, game ID `game-42` is the component
`game%2D42`, producing the payload `getgamestate-game%2D42`.

An empty component represents an omitted optional field between later fields.
Trailing optional fields are omitted entirely. Unless a message definition says
otherwise, integers are unsigned decimal with no leading plus sign, booleans are
`0` or `1`, identifiers are opaque, and a component called `data`, `state`,
`changes`, `arguments`, or `metadata` is opaque text defined by the selected
game type and rules version.

For example:

```text
getgamestate-42
message-42-msg81-game-Good%20game%21
other-42-action82-124-bid-3%20hearts
```

These are compact payload strings, not JSON documents. The surrounding envelope
remains JSON.

A sender MUST emit the exact number and order of components defined for a verb.
Empty required components, bad percent escapes, invalid UTF-8, extra components,
and unknown verbs are `invalidmessage` or `unsupportedmessagetype` errors.

### 7.2 JSON object

An operation object payload MUST contain a string `type` whose
value is the same lowercase verb used by the compact form. The angle-bracketed
component names in sections 8 through 10 become case-sensitive JSON member
names. For example, the compact request `getgamestate-42` is equivalent to:

```json
{
  "type": "getgamestate",
  "gameId": "42"
}
```

Optional compact components become optional JSON members. Integers become JSON
numbers and MUST be non-negative safe integers; `0`/`1` flags become JSON
booleans. Identifiers, names, hashes, timestamps, status values, reasons, and
human-readable text remain strings without percent encoding. `state`, `changes`,
`data`, `arguments`, `options`, `filter`, `metadata`, and `details` MAY be any
JSON value allowed by their game-specific schema.

Unknown members SHOULD be ignored unless the selected game schema declares them
invalid. Duplicate member names, missing required members, wrong types, and
non-finite or out-of-range numbers are invalid. Envelope JSON serialization
SHOULD be compact when bandwidth matters.

## 8. Client requests and actions

### 8.1 Discovery and membership

Angle brackets below describe one encoded component and are not sent. Square
brackets identify optional trailing components.

| Request payload | Server result verb | Cardinality |
| --- | --- | --- |
| `listgames[-<cursor>-<limit>-<filter>]` | `gamelist` | unary |
| `subscribegamelist[-<afterRevision>-<filter>]` | `gamelist` then `gamelistupdate` | streaming |
| `getgame-<gameId>` | `game` | unary |
| `creategame-<gameType>-<rulesVersion>[-<visibility>-<options>]` | `gamecreated` | unary |
| `joingame-<gameId>[-<role>-<seat>-<displayName>]` | `joinedgame` | unary |
| `leavegame-<gameId>[-<actionId>]` | `leftgame` | unary |

Optional fields before a supplied later field use an empty component. Thus
`listgames--25-euchre` omits `cursor`, limits the result to 25, and supplies the
filter text `euchre`. `role` commonly decodes to `player` or `spectator`;
deployments MAY define others. Joining does not imply a state subscription.

### 8.2 State and synchronization

| Request payload | Server result verb | Cardinality |
| --- | --- | --- |
| `getgamestate-<gameId>[-<knownRevision>]` | `gamestate` or `stateunchanged` | unary |
| `getgamehash-<gameId>[-<revision>]` | `gamehash` | unary |
| `subscribegamestate-<gameId>[-<afterRevision>-<preferDeltas>]` | `gamestate` and/or `gamestateupdate` | streaming |

The example `getgamestate-42` requests the current projected state of game 42.
`subscribegamestate-42--1` omits `afterRevision` and requests deltas after the
initial snapshot when possible.

State and hash results MUST be computed for the requesting viewer's authorized
projection, never blindly from server-private state. The state-management
specification defines snapshots, deltas, revisions, and knowledge boundaries.

### 8.3 Actions

`playcard` expresses moving or playing a card:

```text
playcard-<gameId>-<actionId>-<expectedRevision>-<sourcePile>-<sourceCard>-<targetPile>[-<targetPosition>-<data>]
```

For example:

```text
playcard-42-client9_action81-123-2-4-7
```

`sourcePile`, `sourceCard`, `targetPile`, and optional `targetPosition` are
indexes unless the negotiated game schema says they are identifiers. This
example selects card 4 of pile 2 and targets pile 7. Optional `data` may carry a
game-defined count, face, orientation, or other play information.

The server MUST resolve every reference against the actor's projected state at
`expectedRevision` and verify that the card is visible/selectable and the move
is legal. An omitted expected revision is written as an empty component, but
state-changing clients SHOULD always supply one.

`other` carries a game-specific action that is not naturally a card play:

```text
other-<gameId>-<actionId>-<expectedRevision>-<name>[-<arguments>]
```

For example, where the euchre rules define the argument text `3 hearts`:

```text
other-42-client9_action82-124-bid-3%20hearts
```

Examples of decoded `name` values include `bid`, `pass`, `choosetrump`,
`goalone`, `declaremeld`, `draw`, `deal`, `discard`, `selectpasscards`, `claim`,
`concede`, `ready`, and `startnextround`. Names and argument text are defined by
the game type and rules version. They MUST NOT be treated as trusted procedure
names or dynamically executed by a server.

Every state-changing operation SHOULD carry an `actionId` and
`expectedRevision`. Retrying the same action ID with semantically equivalent
decoded content after documented normalization MUST return the original outcome
without applying the action twice. Reuse with different content MUST fail with
`actionidconflict`.

Action results use:

```text
actionresult-<gameId>-<actionId>-<accepted>[-<revision>-<reason>]
```

An accepted example is `actionresult-42-client9_action81-1-124`. A predictable
rule rejection MAY set `accepted` to `0` and include a non-sensitive reason.
Malformed, unauthorized, conflicting, or unsupported operations SHOULD use the
terminal `error` envelope described in section 10.

The result may be sent in `next` followed by an empty `complete`, or directly in
the payload of server `complete`.

### 8.4 Human-readable messages

`subscribemessages-<gameId>[-<scope>-<afterMessageId>]` starts a message stream.
Chat history retention and replay are deployment policy, so a server MAY begin
with only new messages.

A client sends:

```text
message-<gameId>-<actionId>-<scope>-<text>
```

For example, `message-42-msg81-game-Good%20game%21` has decoded text
`Good game!`. The server returns `messageresult` to the sender. Delivered
messages use the `message` result form in section 9.

A server MUST authorize the scope, assign the canonical sender and timestamp,
enforce size/rate limits, and not trust client-supplied sender identity. Game
actions or secret card data MUST NOT be tunneled through human-readable messages.

## 9. Server result messages

The server sends these payload forms as `next` payloads or as the optional final
payload of server `complete`:

| Result form | Meaning |
| --- | --- |
| `game-<gameId>-<gameType>-<rulesVersion>-<status>-<playerCount>-<seatCapacity>[-<metadata>]` | Public metadata for one game. |
| `gamecreated-<gameId>-<gameType>-<rulesVersion>` | Newly created game. |
| `joinedgame-<gameId>-<role>[-<seat>-<resumeToken>]` | Accepted membership. |
| `leftgame-<gameId>` | Membership ended. |
| JSON `gamestate` object defined below | Complete viewer snapshot. |
| `stateunchanged-<gameId>-<revision>-<hash>` | The supplied known revision is current. |
| `gamestateupdate-<gameId>-<baseRevision>-<revision>-<knowledgeEpoch>-<hash>-<changes>` | Viewer-specific delta. |
| `gamehash-<gameId>-<revision>-<hashAlgorithm>-<hash>` | Hash of the viewer projection. |
| `actionresult-<gameId>-<actionId>-<accepted>[-<revision>-<reason>]` | Action outcome. |
| `messageresult-<gameId>-<actionId>-<accepted>[-<messageId>-<reason>]` | Message-send outcome. |
| `message-<gameId>-<messageId>-<scope>-<sender>-<timestamp>-<text>` | Canonical delivered message. |

A complete state snapshot MUST use a JSON object payload. The `state` member is
the actual game-specific JSON value, not JSON serialized into another string:

```json
{
  "type": "gamestate",
  "gameId": "42",
  "gameType": "euchre",
  "rulesVersion": "1.0",
  "revision": 124,
  "knowledgeEpoch": 7,
  "hashAlgorithm": "jcssha256",
  "hash": "viewer-projection-hash",
  "state": {
    "phase": "play",
    "activeSeat": 2,
    "piles": []
  }
}
```

All shown members are required except `knowledgeEpoch`, which MAY be omitted
when the game has no trackable hidden-card references. `knowledgeEpoch` may be a
non-negative safe integer or opaque string. The schema of `state` is fixed by
`gameType` and `rulesVersion`.

A state delta SHOULD use the corresponding JSON form when `changes` is
structured:

```json
{
  "type": "gamestateupdate",
  "gameId": "42",
  "baseRevision": 124,
  "revision": 125,
  "knowledgeEpoch": 7,
  "hashAlgorithm": "jcssha256",
  "hash": "new-viewer-projection-hash",
  "changes": []
}
```

The compact `gamestateupdate` form in the table is allowed when the selected
game rules define `changes` as one canonical text value.

`gamelist` has this form:

```text
gamelist-<listRevision>-<nextCursor>-<count>-<record>...
```

Each record is exactly seven components in this order: `gameId`, `gameType`,
`rulesVersion`, `status`, `playerCount`, `seatCapacity`, and optional public
`metadata` (represented by an empty component when absent). Exactly `count`
records MUST follow. An absent next cursor is an empty component.

In JSON form, `gamelist` uses `listRevision`, optional `nextCursor`, and a
`games` array. Each array item is an object with `gameId`, `gameType`,
`rulesVersion`, `status`, `playerCount`, `seatCapacity`, and optional `metadata`.
The compact `count` is replaced by the JSON array length.

`gamelistupdate` has this form:

```text
gamelistupdate-<listRevision>-<count>-<change>...
```

Each change is exactly three components: `kind`, `gameId`, and `metadata`.
`kind` is `add`, `change`, or `remove`; metadata is empty for `remove` and is the
game-summary text defined by the service otherwise. Exactly `count` changes
MUST follow.

In JSON form, `gamelistupdate` uses `listRevision` and a `changes` array. Each
item has `kind`, `gameId`, and optional `metadata`; the compact `count` is
replaced by the JSON array length.

In compact form, `changes`, `arguments`, and `metadata` are opaque game-defined
text after percent-decoding. In JSON form they are structured values rather than
JSON encoded inside strings. A client that does not understand the negotiated
game type can browse generic public metadata but cannot safely interpret or
mutate game state.

If `subscribegamestate` specifies `afterRevision` and the server retains all
authorized deltas after it, the server MAY replay them. Otherwise it MUST first
send a current `gamestate` snapshot. Updates on one operation MUST be revision
ordered and MUST NOT omit a revision that changes that viewer's projection.

## 10. Errors

An `error` envelope terminates exactly one operation. Its compact payload is:

```text
error-<code>-<retryable>-<message>[-<details>]
```

For example:

```text
error-revisionconflict-1-The%20game%20changed%20before%20the%20action%20was%20applied.-125
```

The equivalent JSON form is useful when `details` is structured:

```json
{
  "type": "error",
  "code": "revisionconflict",
  "retryable": true,
  "message": "The game changed before the action was applied.",
  "details": { "currentRevision": 125 }
}
```

Recommended codes include `invalidmessage`, `unsupportedmessagetype`,
`notauthenticated`, `notauthorized`, `gamenotfound`, `gamefull`, `invalidrole`,
`invalidaction`, `illegalmove`, `notyourturn`, `revisionconflict`,
`actionidconflict`, `ratelimited`, and `internalerror`.

`details` is optional code-specific text. Error message and details MUST be safe
for the requesting viewer. In particular, they MUST NOT reveal hidden card
identities, another player's pending choice, the canonical state hash, or
whether a guessed hidden card was correct. Clients MUST branch on `code`, not
localized `message` text.

Malformed connection-level messages may require closing the WebSocket rather
than sending an operation error. Suggested close codes, aligned where useful
with `graphql-transport-ws`, are:

| Code | Meaning |
| --- | --- |
| `4400` | Invalid envelope or unsupported protocol message. |
| `4401` | Unauthorized or operation sent before acknowledgement. |
| `4403` | Forbidden initialization. |
| `4408` | Initialization timeout. |
| `4409` | Duplicate active operation identifier. |
| `4429` | Too many initialization requests. |

## 11. Concurrency, ordering, and backpressure

- The server is authoritative. Receipt of an action is not acceptance.
- Concurrent actions MUST be serialized into a deterministic revision order or
  rejected according to the game concurrency policy.
- An action result and subscription update can travel on different operation
  identifiers. Clients MUST use `actionId` and revisions rather than arrival
  order to correlate them.
- Slow consumers MUST be bounded. A server MAY coalesce replaceable state
  updates, switch a client to a fresh snapshot, terminate an operation with
  `ratelimited`, or close a persistently unhealthy connection.
- A state delta MUST never be coalesced in a way that changes its meaning. If a
  continuous delta chain cannot be preserved, send a snapshot.

## 12. Security and privacy

- Production connections MUST use `wss`.
- The server MUST authenticate and authorize each operation; acknowledgement
  alone does not permanently authorize every game or action.
- State, hashes, errors, identifiers, counts, deltas, and timing MUST respect the
  viewer's knowledge and spectator policy.
- A client-supplied game type, rules version, pile index, action name, compact
  component, percent-encoded byte, object member, or nested value is untrusted
  input.
- Implementations SHOULD limit nesting depth, string length, list length,
  operation count, messages per second, and decompressed WebSocket size.
- Servers SHOULD use generic responses where a more precise error would reveal
  private state.

## 13. Minimal conformance requirements

A conforming CGWS implementation:

1. negotiates `card-game-transport-ws` and uses only WebSocket text messages;
2. implements `connection_init`, `connection_ack`, `ping`, `pong`, `subscribe`,
   `next`, `error`, and `complete` with the lifecycle above;
3. accepts only string or object payloads and uses each message's required form;
4. supports interleaved operations with unique string identifiers;
5. implements or explicitly negotiates the `card-game-v1` compact and JSON
   payload formats;
6. preserves operation and revision ordering;
7. prevents unauthorized information from entering results or errors; and
8. documents supported game types, rules versions, state schemas, extension
   actions, limits, and reconnect behavior.
