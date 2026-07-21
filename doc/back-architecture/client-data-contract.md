# Client data contract and implementation guide

## Connection model

Connect a Socket.IO client to the backend origin (normally `http://localhost:3000`). The server permits only the configured `CLIENT_URL` origin. WebSocket is supported; the included client uses `websocket` with polling fallback.

Use one stable, client-generated `playerId` per player (the supplied frontend uses `crypto.randomUUID()` and browser local storage). Keep the `roomId` returned by the server and reuse the same `playerId` after a transport reconnect.

```ts
import { io } from "socket.io-client";

const socket = io("http://localhost:3000", {
  transports: ["websocket", "polling"],
});

socket.emit("create-room", { playerId: "player-alice" });
socket.on("room-created", ({ roomId, player }) => {
  // Persist roomId and player.symbol (X).
});
```

All Socket.IO messages are JSON-compatible objects. There are no acknowledgement callbacks, so install listeners before emitting commands. `turnDeadline` is an absolute Unix epoch timestamp in **milliseconds**, not a duration. A client countdown is `Math.max(0, turnDeadline - Date.now())`.

## Shared field shapes

```json
{
  "player": {
    "playerId": "player-alice",
    "symbol": "X",
    "connected": true
  },
  "board": [
    ["X", null, null],
    [null, "O", null],
    [null, null, null]
  ],
  "move": {
    "playerId": "player-alice",
    "row": 0,
    "col": 0,
    "timestamp": "2026-07-21T12:00:05.000Z"
  }
}
```

- `symbol` is `"X"` or `"O"`.
- `board` is always 3 rows of 3 cells; each cell is `"X"`, `"O"`, or `null`.
- `row` and `col` are zero-based integers in `0..2`.
- Player summaries never expose `socketId` or `joinedAt`.
- Timestamps in Socket.IO move payloads are ISO-8601 strings; REST date fields are also JSON ISO strings.
- `winningCells`, when present, is three `[row, col]` pairs.

Example IDs and timestamps below are illustrative.

## REST data sent to the client

REST is read-oriented; gameplay room creation happens through Socket.IO. Send `Accept: application/json`; JSON request bodies are accepted by Express but no current REST route needs one.

### `GET /health`

Response `200`:

```json
{
  "status": "ok",
  "server": "running"
}
```

### `GET /rooms`

Returns `200` and an array of **open/waiting** rooms only. It is suitable for a lobby. It excludes `ACTIVE` and `FINISHED` games.

```json
[
  {
    "roomId": "3f1c2d52-2faa-4c36-a41f-5bd47b99c701",
    "status": "OPEN",
    "playerCount": 1,
    "players": [
      { "playerId": "player-alice", "symbol": "X", "connected": true }
    ],
    "createdAt": "2026-07-21T12:00:00.000Z",
    "updatedAt": "2026-07-21T12:00:00.000Z"
  }
]
```

An empty lobby returns `[]`, not 404.

### `GET /rooms/:id`

Returns `200` and the current room plus a complete game snapshot. This endpoint is not access-controlled; any caller with a room ID can read it.

```json
{
  "roomId": "3f1c2d52-2faa-4c36-a41f-5bd47b99c701",
  "status": "ACTIVE",
  "players": [
    { "playerId": "player-alice", "symbol": "X", "connected": true },
    { "playerId": "player-bob", "symbol": "O", "connected": true }
  ],
  "game": {
    "board": [["X", null, null], [null, "O", null], [null, null, null]],
    "currentPlayer": { "playerId": "player-alice", "symbol": "X", "connected": true },
    "turnDeadline": 1784635260000,
    "winner": null,
    "winningCells": null,
    "isDraw": false,
    "moves": [
      { "playerId": "player-alice", "row": 0, "col": 0, "timestamp": "2026-07-21T12:00:05.000Z" },
      { "playerId": "player-bob", "row": 1, "col": 1, "timestamp": "2026-07-21T12:00:11.000Z" }
    ],
    "startedAt": "2026-07-21T12:00:00.000Z",
    "completedAt": null
  },
  "createdAt": "2026-07-21T11:59:56.000Z",
  "updatedAt": "2026-07-21T12:00:00.000Z"
}
```

For an `OPEN` room, `game.board` is an empty 3×3 board and `currentPlayer`, `turnDeadline`, `winner`, `startedAt`, and `completedAt` are `null`. A missing ID returns `404`:

```json
{ "error": "Room not found: unknown-room" }
```

### `POST /rooms`

This route currently always returns `501`:

```json
{ "error": "Not implemented" }
```

Use the `create-room` Socket.IO event instead. An unmatched HTTP path returns `404 { "error": "Route not found" }`.

## Socket.IO command and event reference

`Client → server` denotes an event the browser emits. `Server → client` describes who receives it: **sender**, **joiner**, or **room** (all sockets joined to the room, including the sender where applicable).

| Direction | Event | Recipient | Purpose |
|---|---|---|---|
| Client → server | `create-room` | — | Create an open room and become X. |
| Server → client | `room-created` | sender | Creation confirmation and assigned X player. |
| Client → server | `join-room` | — | Join an open room and become O. |
| Server → client | `room-joined` | joiner | Join confirmation and assigned O player. |
| Server → client | `player-joined` | room | Player roster change. |
| Server → client | `game-started` | room | Full initial active-game state. |
| Client → server | `make-move` | — | Request a move. |
| Server → client | `move-result` | room | Full board result of valid, invalid, finished, or timeout move processing. |
| Native Socket.IO → server | `disconnect` | — | Socket disconnect notification. |
| Server → client | `player-disconnected` | room | A known player's connection became inactive. |
| Client → server | `reconnect` | — | Rebind stable player identity to a new socket. |
| Server → client | `room-state` | reconnecting socket | Full authoritative snapshot after reconnection. |
| Server → client | `player-reconnected` | room | Player is connected again. |
| Server → client | `error` | initiating socket | Command failure that did not use `move-result`. |

### Create a room

Emit:

```json
{ "playerId": "player-alice" }
```

Receive `room-created` (sender only):

```json
{
  "roomId": "3f1c2d52-2faa-4c36-a41f-5bd47b99c701",
  "player": { "playerId": "player-alice", "symbol": "X", "connected": true }
}
```

Common failure (`error`, sender only):

```json
{ "message": "Player already in a room: player-alice" }
```

The server also rejects a socket already indexed to a room. Room IDs are server-generated UUID v4 values.

### Join a room and receive initial game state

Emit:

```json
{
  "roomId": "3f1c2d52-2faa-4c36-a41f-5bd47b99c701",
  "playerId": "player-bob"
}
```

The joiner first receives `room-joined`:

```json
{
  "roomId": "3f1c2d52-2faa-4c36-a41f-5bd47b99c701",
  "player": { "playerId": "player-bob", "symbol": "O", "connected": true }
}
```

Both players then receive `player-joined`:

```json
{
  "playerId": "player-bob",
  "symbol": "O",
  "players": [
    { "playerId": "player-alice", "symbol": "X", "connected": true },
    { "playerId": "player-bob", "symbol": "O", "connected": true }
  ]
}
```

Both players receive `game-started`:

```json
{
  "board": [[null, null, null], [null, null, null], [null, null, null]],
  "currentPlayer": { "playerId": "player-alice", "symbol": "X", "connected": true },
  "turnDeadline": 1784635230000,
  "playerX": { "playerId": "player-alice", "symbol": "X", "connected": true },
  "playerO": { "playerId": "player-bob", "symbol": "O", "connected": true }
}
```

Joining an unknown, full, or otherwise invalid room sends `error` only to the joiner, for example `{ "message": "Room is full: 3f1c..." }`.

### Make a move

Emit a zero-based target coordinate:

```json
{
  "roomId": "3f1c2d52-2faa-4c36-a41f-5bd47b99c701",
  "playerId": "player-alice",
  "row": 0,
  "col": 0
}
```

For a valid non-final move, every socket in the room receives `move-result`:

```json
{
  "success": true,
  "move": {
    "playerId": "player-alice",
    "row": 0,
    "col": 0,
    "timestamp": "2026-07-21T12:00:05.000Z"
  },
  "board": [["X", null, null], [null, null, null], [null, null, null]],
  "currentPlayer": { "playerId": "player-bob", "symbol": "O", "connected": true },
  "turnDeadline": 1784635260000,
  "winner": null,
  "isDraw": false
}
```

For a winning move, `currentPlayer` and `turnDeadline` become `null` and the winning cells are provided:

```json
{
  "success": true,
  "move": { "playerId": "player-alice", "row": 0, "col": 2, "timestamp": "2026-07-21T12:00:23.000Z" },
  "board": [["X", "X", "X"], ["O", "O", null], [null, null, null]],
  "currentPlayer": null,
  "turnDeadline": null,
  "winner": { "playerId": "player-alice", "symbol": "X", "connected": true },
  "winningCells": [[0, 0], [0, 1], [0, 2]],
  "isDraw": false
}
```

A draw has `winner: null`, `currentPlayer: null`, `turnDeadline: null`, and `isDraw: true` (the server includes `winningCells: null` for this path).

Invalid moves also produce a room-wide `move-result`, not an `error` event. Example for an occupied cell:

```json
{
  "success": false,
  "error": "Cell (0, 0) is already occupied",
  "board": [["X", null, null], [null, null, null], [null, null, null]],
  "currentPlayer": { "playerId": "player-bob", "symbol": "O", "connected": true },
  "turnDeadline": 1784635260000,
  "winner": null,
  "isDraw": false
}
```

Other possible messages include `It is not your turn`, `Position (3, 0) is out of bounds`, `Game is already over`, `Game has not started`, `Game is not active`, and `Room not found: <id>`. Do not replace local board state on a failed event; use its board/current-turn fields to resynchronize. The runtime omits `winningCells` unless it is non-null, so treat it as optional.

### Timeout result

When the scheduled turn timer fires, the room receives:

```json
{
  "success": true,
  "board": [["X", null, null], [null, null, null], [null, null, null]],
  "winner": { "playerId": "player-bob", "symbol": "O", "connected": true },
  "winningCells": null,
  "isDraw": false,
  "currentPlayer": null,
  "turnDeadline": null,
  "timeoutWin": true
}
```

There is no `move` because no board cell was played. `timeoutWin` is present only for the scheduled-timer route; it is an optional field outside the current exported TypeScript payload type.

### Disconnect and reconnect

When a player's known socket disconnects, the room receives:

```json
{ "playerId": "player-bob" }
```

The client reconnects after a new transport connection by emitting:

```json
{ "playerId": "player-bob" }
```

If found, the reconnecting socket receives `room-state`, which is the authoritative restore payload:

```json
{
  "roomId": "3f1c2d52-2faa-4c36-a41f-5bd47b99c701",
  "status": "ACTIVE",
  "players": [
    { "playerId": "player-alice", "symbol": "X", "connected": true },
    { "playerId": "player-bob", "symbol": "O", "connected": true }
  ],
  "board": [["X", null, null], [null, "O", null], [null, null, null]],
  "currentPlayer": { "playerId": "player-alice", "symbol": "X", "connected": true },
  "turnDeadline": 1784635260000,
  "winner": null,
  "winningCells": null,
  "isDraw": false,
  "moves": [
    { "playerId": "player-alice", "row": 0, "col": 0, "timestamp": "2026-07-21T12:00:05.000Z" },
    { "playerId": "player-bob", "row": 1, "col": 1, "timestamp": "2026-07-21T12:00:11.000Z" }
  ]
}
```

The room then receives `player-reconnected`:

```json
{ "playerId": "player-bob" }
```

If the player ID has no in-memory room, only the initiating socket receives:

```json
{ "message": "No active game found for reconnection" }
```

Despite the wording, reconnection can restore an open or finished room too if that player remains indexed in memory.

## Client-state handling rules

1. Treat server board, player list, winner, draw status, and deadline as authoritative. Do not derive a board solely from local click history.
2. Save the stable `playerId`, `roomId`, and assigned symbol on `room-created`, `room-joined`, or `room-state`.
3. Start/refresh a countdown on `game-started` and successful active `move-result`; clear it whenever `currentPlayer` is `null`, a winner exists, or `isDraw` is true.
4. Enable a board cell only when the game is active, the current player's `playerId` equals the local `playerId`, and the board cell is `null`. The server remains the final validator.
5. On `room-state`, replace local game state wholesale; this is the resynchronization message after reconnect.
6. Display `error` without changing game state. For failed `move-result`, keep/reconcile state from its supplied board and current player rather than applying the attempted move.
7. Be prepared for optional fields. In particular, a normal `move-result` has no `winningCells` unless there is a win; a timer result has no `move`; `timeoutWin` may be absent even when a timeout ended the game.

## Current contract caveats

- The actual event payloads are runtime behavior. `src/types/gameEvents.ts` is incomplete: it omits `room-joined`, declares a `move.symbol` that the server does not send, and lacks `timeoutWin`.
- Inputs are not runtime-validated and client identity is not authenticated. Treat this server as a trusted-development protocol, not a secure public API.
- No REST or socket acknowledgement writes durable data. A backend restart clears every room and invalidates reconnect state.
