# Backend architecture

## Purpose and boundary

`backend/` implements a two-player, real-time Tic-Tac-Toe server using Node.js, Express 4, Socket.IO 4, and TypeScript. One HTTP server hosts two transports:

```text
Browser / client
  |-- HTTP REST ----------------------------> Express routes
  |                                              |-- health and room read models
  |
  `-- Socket.IO (WebSocket; polling fallback) -> socket handlers
                                                 |-- GameManager (application facade)
                                                 |    |-- RoomManager (in-memory lobby/indexes)
                                                 |    `-- GameEngine (rules and mutable game state)
                                                 |         `-- WinnerChecker (pure 8-line evaluator)
                                                 `-- Socket.IO room broadcasts
```

The included `frontend/index.html` is a diagnostic/game client. It connects to `http://localhost:3000`, retains a generated `playerId` in browser local storage, calls the three GET REST endpoints, and sends/consumes the Socket.IO events documented in [client-data-contract.md](./client-data-contract.md).

## Runtime composition

| Layer | Main files | Responsibility |
|---|---|---|
| Bootstrap | `src/server.ts` | Creates one Node `http.Server`, attaches Socket.IO, and listens on `env.port`. |
| HTTP application | `src/app.ts` | Configures CORS, JSON parsing, routes, 404 handling, and error-to-JSON conversion. |
| REST | `routes/*`, `controllers/RoomController.ts` | Exposes health, open-room discovery, and a full room/game snapshot. It obtains the same `GameManager` created for Socket.IO. |
| Realtime transport | `socket/socket.ts`, `socket/handlers/*` | Registers per-connection event handlers, maintains server-side timer handles, joins Socket.IO rooms, and emits/broadcasts messages. |
| Application facade | `game/GameManager.ts` | Coordinates room lifecycle, game initialization, moves, finished state, and connected/disconnected player state. Reads timeout configuration. |
| Domain | `game/RoomManager.ts`, `GameEngine.ts`, `WinnerChecker.ts` | Owns mutable room/game state, indexes, rule validation, turn switching, winning/draw calculation, and timeout resolution. |
| Models/types | `models/*`, `types/*` | Defines TypeScript shapes and event-name constants. These types are not runtime validators. |
| Cross-cutting | `config/env.ts`, `middleware/*`, `utils/*` | Environment values, CORS/error middleware, UUID generation, and console logging. |

## Startup and configuration

`src/config/env.ts` loads `.env` through `dotenv`. Defaults are below.

| Variable | Default | Used for |
|---|---:|---|
| `PORT` | `3000` | HTTP and Socket.IO listening port. |
| `CLIENT_URL` | `http://localhost:5173` | Allowed CORS origin for Express and Socket.IO. |
| `TURN_TIMEOUT` | `30000` | Turn duration in milliseconds. |
| `TURN_TIMEOUT_ENABLED` | `true` | Timeout is disabled only when the exact value is `false`. |

On startup `server.ts` creates Express first, wraps it in the HTTP server, and calls `initializeSocket(server)`. That call constructs the singleton `GameManager` used by both Socket.IO handlers and `RoomController`. Calling REST controller methods before Socket.IO initialization would throw, but the normal startup sequence prevents that.

## State model and lifecycle

### Room and player state

`RoomManager` owns all persistent-in-process data:

```text
rooms: Map<roomId, Room>
playerToRoom: Map<playerId, roomId>
socketToRoom: Map<socketId, roomId>
```

Each `Room` has a UUID v4 `roomId`, `players`, a `GameEngine`, timestamps, and one of these statuses:

```text
OPEN --(second player joins)--> ACTIVE --(win, draw, or timeout)--> FINISHED
```

The first player is always `X`; the second is always `O`. A player ID and a socket ID can each be indexed to only one room. A room accepts at most two players. `leaveRoom` and room deletion exist in the domain layer but are not exposed through HTTP or Socket.IO in the current server.

Disconnecting does **not** remove the player or stop a game. It changes `Player.connected` to `false`. Reconnection finds the room by `playerId`, replaces the stored `socketId`, marks the player connected, and returns a full state snapshot.

### Game state

`GameEngine` is created with the room but initialized only when a second player joins. It owns:

- a 3 × 3 `(string | null)[][]` board (`"X"`, `"O"`, or `null`);
- append-only move history with server-created `Date` timestamps;
- player references, turn index, start/completion time, and an absolute Unix-millisecond `turnDeadline`;
- winner, winning cells, draw flag, and timeout settings.

The engine validates a submitted move in this order: game started, game not over, row/column bounds, empty cell, current player exists, matching `playerId`, then deadline. On success it writes the board, appends a move, checks eight winning lines, and either ends the game, declares a draw, or switches turn and establishes the next deadline. Board getters return cloned rows, avoiding direct board mutation by callers.

`WinnerChecker` is deterministic and has no state. It checks three rows, three columns, and two diagonals; if none wins, a full board is a draw.

## Main execution flows

### Create, join, and start

1. A connected socket sends `create-room` with its client-created `playerId`.
2. `RoomManager` creates an `OPEN` room, adds an `X` player, and indexes the player/socket.
3. The handler joins the socket to the Socket.IO room and sends `room-created` only to that socket.
4. A second socket sends `join-room` with that `roomId` and another `playerId`.
5. `RoomManager` adds `O`, changes the room to `ACTIVE`, and `GameManager` initializes the game with both players and configured timeout values.
6. The joiner is added to the Socket.IO room. It receives `room-joined`; both players receive `player-joined` and then `game-started`.
7. If enabled, `scheduleTurnTimeout` creates a Node timer for X's initial deadline.

### Move and result

1. Any connected socket sends `make-move` with `roomId`, `playerId`, `row`, and `col`.
2. `GameManager` loads the room and calls the engine. Domain exceptions are converted to a `success: false` result.
3. The handler broadcasts `move-result` to the Socket.IO room named by the supplied `roomId`; valid moves update all clients with the full board and next turn/end state.
4. A winning move or draw changes room status to `FINISHED` and clears the scheduled timer. A normal move replaces the timer with one for the new deadline.

The API does not use Socket.IO acknowledgement callbacks. The resulting `move-result` event is the client response, and it is broadcast to both players, including the sender.

### Timeout

Each active room has at most one process-local `setTimeout` handle in `turnTimeouts`. The handler clears a prior handle before scheduling a new one. On expiry, `GameManager.checkTimeout` makes the non-current player the winner, marks the room `FINISHED`, and sends a `move-result` with `timeoutWin: true`.

There is a secondary path: if a late `make-move` reaches the engine after its deadline but before the scheduled callback resolves it, the engine resolves the timeout during that call. That response is still a successful `move-result`, but the move is absent and `timeoutWin` is absent. Clients should therefore use `winner !== null`, `isDraw`, and `currentPlayer === null` as the authoritative end-state signals; `timeoutWin` is only an optional UI hint.

### Disconnect and reconnect

On native Socket.IO `disconnect`, the server looks up the socket, marks the matching player disconnected, and broadcasts `player-disconnected`. A client reconnects by sending custom `reconnect` with its stable `playerId`; the server replaces the old socket ID, joins the new socket to the room, sends `room-state` to that socket, and broadcasts `player-reconnected` to the room (including the reconnecting socket).

No pause, forfeit, or grace-period rule is implemented: turn deadlines continue while a player is offline.

## HTTP API implementation

| Method/path | Behavior |
|---|---|
| `GET /health` | Stateless liveness response `{ "status": "ok", "server": "running" }`. |
| `GET /rooms` | Returns summaries for `OPEN` rooms only; active and finished rooms are intentionally excluded. |
| `GET /rooms/:id` | Returns a live room/game snapshot, including move history. A missing room produces `404`. |
| `POST /rooms` | Deliberately stubbed: always returns `501 { "error": "Not implemented" }`. Create rooms through Socket.IO. |

Unknown paths return `404 { "error": "Route not found" }`. The global error middleware maps `RoomNotFoundError` and `PlayerNotInRoomError` to 404, and `RoomFullError`/`PlayerAlreadyInRoomError` to 409; unrecognized errors become 500. REST response dates are serialized by Express as ISO-8601 strings.

## Deployment and scaling

The project includes a multi-stage Node 20 Alpine Dockerfile: it builds TypeScript in a builder image and runs `dist/server.js` with production dependencies. `docker-compose.yml` exposes port 3000, consumes `.env`, and mounts the backend directory for development-style operation.

This implementation is designed for one process:

- Rooms, player indexes, move history, and timer handles are in memory only. Restarting loses all games and reconnectability.
- Socket.IO rooms are local to one process. With multiple instances, a client may reach a different instance and see no room.
- Turn timers are local Node timers, not durable jobs.

For horizontally scaled or recoverable production use, move room/game state to a durable store, use a Socket.IO adapter (for example Redis) for broadcasts, coordinate timers in a durable worker/job system, and restore state before accepting reconnections.

## Security and implementation observations

The server has CORS origin configuration but no authentication, authorization, rate limiting, schema validation, or persistence. `playerId` is supplied by the client and is treated as the player identity. The move handler validates the supplied player ID against whose turn it is, but does not prove that the emitting socket owns that player ID; a client that knows another player's ID can impersonate it. Likewise, outbound move results target the supplied room ID. An authenticated session/socket-to-player authorization check should precede game commands.

Payload types in `src/types/gameEvents.ts` document intent but are not used to type the Socket.IO server or validate runtime input. There are current source/contract drifts: `room-joined` is emitted but absent from `SocketEvents` and `ServerEvents`; `MoveSummary` declares `symbol`, but actual move events omit it; scheduled timeout events include undeclared `timeoutWin`. The client guide records the actual emitted contract.

There are no automated tests in the repository. Recommended coverage begins with engine rule tests, room-index/lifecycle tests, Socket.IO integration tests for broadcasts and failures, and fake-timer tests for deadline behavior.
