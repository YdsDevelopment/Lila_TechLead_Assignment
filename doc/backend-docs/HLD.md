# High-Level Design: Tic-Tac-Toe Backend

## 1. System Overview

Multiplayer Tic-Tac-Toe game server supporting real-time gameplay via WebSocket (Socket.IO). Players create or join rooms, take turns making moves, and receive live board updates. The server manages game state, turn timers, disconnection handling, and reconnection.

---

## 2. Tech Stack

| Component | Technology |
|---|---|
| Runtime | Node.js 20 (Alpine Docker) |
| Language | TypeScript 5.5 |
| Web Framework | Express 4.21 |
| Realtime | Socket.IO 4.7 |
| ID Generation | uuid v10 |
| Config | dotenv |
| Dev Runner | ts-node-dev (hot reload) |
| Linting | ESLint 9 |
| Formatting | Prettier |

---

## 3. Architecture

### 3.1 Layered Architecture

```
┌─────────────────────────────────────────────┐
│              Transport Layer                 │
│  ┌──────────┐  ┌──────────┐  ┌───────────┐  │
│  │ Socket.IO │  │ Express  │  │  Static   │  │
│  │ (realtime)│  │  (REST)  │  │  Frontend │  │
│  └────┬─────┘  └────┬─────┘  └───────────┘  │
├───────┴──────────────┴───────────────────────┤
│            Handler / Controller Layer         │
│  ┌─────────────────────────────────────────┐  │
│  │  Socket Handlers (10)                   │  │
│  │  REST Controllers (RoomController)      │  │
│  └────────────────┬────────────────────────┘  │
├───────────────────┴──────────────────────────┤
│              Service / Game Layer             │
│  ┌──────────────┐  ┌────────────┐             │
│  │  GameManager │  │ RoomManager│             │
│  │   (facade)   │  │  (CRUD)    │             │
│  └──────┬───────┘  └─────┬──────┘             │
│  ┌──────┴──────────────┐ │                    │
│  │     GameEngine      │◄┘                    │
│  │  (per-room state)   │                      │
│  └─────────┬───────────┘                      │
│  ┌─────────┴───────────┐                      │
│  │    WinnerChecker    │                      │
│  │    (stateless)      │                      │
│  └─────────────────────┘                      │
├───────────────────────────────────────────────┤
│              Data / Model Layer                │
│  ┌───────┐ ┌──────┐ ┌────────┐ ┌───────────┐  │
│  │ Room  │ │Player│ │  Move  │ │ GameState │  │
│  └───────┘ └──────┘ └────────┘ └───────────┘  │
└───────────────────────────────────────────────┘
```

### 3.2 In-Memory State Store

All state is held in-memory — no database. The `RoomManager` maintains three maps:

```
rooms: Map<roomId, Room>
playerToRoom: Map<playerId, roomId>
socketToRoom: Map<socketId, roomId>
```

A backend restart clears all state. Reconnect works only for active server sessions.

---

## 4. Component Breakdown

### 4.1 Entry Point (`server.ts`)

Creates an HTTP server from Express app, initializes Socket.IO on top of it, then listens on the configured port.

### 4.2 Express App (`app.ts`)

Minimal Express setup:
- CORS (configured via `CLIENT_URL`)
- JSON body parsing
- REST routes: `/health`, `/rooms`, `/rooms/:id`
- 404 handler + global error handler

### 4.3 Socket.IO Initialization (`socket.ts`)

Creates the `GameManager` singleton, attaches all 10 socket event handlers per connection, starts the global timer broadcast interval (1s ticks for all active rooms), and exports helpers for turn timeout scheduling.

### 4.4 GameManager (Facade)

The single entry point for all game operations. Delegates room CRUD to `RoomManager` and game logic to `GameEngine`. Passes timeout config (from env) to engines on initialization. Methods:

| Method | Description |
|---|---|
| `createRoom` | Create room, return it |
| `joinRoom` | Join room, auto-init game if 2 players |
| `makeMove` | Validate and apply move, return result |
| `playAgain` | Reset game in finished room |
| `leaveRoom` | Remove player, delete room if empty |
| `disconnectPlayer` | Mark player disconnected |
| `reconnectPlayer` | Update socket ID, mark connected |
| `checkTimeout` | Check and resolve turn timeout |
| `getRoom` / `getRoomByPlayer` / `getRoomBySocket` | Lookups |
| `getOpenRooms` / `getActiveRooms` | Room listings |

### 4.5 RoomManager (Data Store)

In-memory room/player/socket indexing with validation:
- No duplicate player or socket across rooms
- Max 2 players per room
- Room lifecycle: OPEN → ACTIVE → FINISHED → (deleted or OPEN on leave)
- Player/socket index cleanup on leave/disconnect

### 4.6 GameEngine (Per-Room Game State)

Each room gets its own `GameEngine` instance containing:
- 3×3 board
- Move history
- Turn tracking (current player index)
- Timer deadline
- win/draw/completion state

Key flow for `makeMove`:
1. Validate game started, not over, valid coordinates, cell empty, correct turn
2. Check timer expiry → auto-resolve timeout (opponent wins)
3. Place symbol, record move
4. Check win/draw via `WinnerChecker`
5. Switch turn, set new deadline
6. Return `MoveResult` with full state

### 4.7 WinnerChecker (Stateless)

Evaluates 8 winning lines on the board:
- 3 rows, 3 columns, 2 diagonals
- Returns winner symbol + winning cells, or draw flag if board full

### 4.8 Socket Handlers (10 handlers)

| Handler | Event | Response | Scope |
|---|---|---|---|
| `createRoom` | `create-room` | `room-created` | Sender |
| `joinRoom` | `join-room` | `room-joined` → joiner, `player-joined` → room, `game-started` → room | Room |
| `makeMove` | `make-move` | `move-result` | Room |
| `reconnect` | `reconnect` | `room-state` → reconnecting socket, `player-reconnected` → room | Both |
| `disconnect` | `disconnect` | `player-left` or `player-disconnected` | Room |
| `playAgain` | `play-again` | `game-started` | Room |
| `leaveRoom` | `leave-room` | `player-left` | Room + leaver |
| `getRooms` | `get-rooms` | `rooms-list` | Sender |
| `getRoom` | `get-room` | `room-details` or `error` | Sender |
| `getHealth` | `get-health` | `health-status` | Sender |

### 4.9 Timer System

Two complementary timer mechanisms:

**Server-side timeout** (`scheduleTurnTimeout` / `clearTurnTimeout`):
- A `setTimeout` per active room, scheduled when turn starts
- On expiry: calls `GameManager.checkTimeout()` which auto-resolves with opponent win
- Cleared on move, game end, leave, play-again, disconnect

**Broadcast timer** (`startTimerBroadcast`):
- Global 1-second `setInterval` iterating all active rooms
- Emits `turn-timer` with `remainingMs` to each room
- Used by clients for UI countdown display

---

## 5. Data Flow Diagrams

### 5.1 Create Room

```
Client A                  Server
   │                        │
   │── create-room ────────►│
   │   { playerId }         │
   │                        ├── RoomManager.createRoom()
   │                        │   ├── Generate roomId (UUID)
   │                        │   ├── Create Player X
   │                        │   ├── Create Room (status=OPEN)
   │                        │   ├── Index player → room
   │                        │   └── Index socket → room
   │                        ├── socket.join(roomId)
   │                        │
   │◄── room-created ───────│
   │   { roomId, player }   │
```

### 5.2 Join Room + Game Start

```
Client A        Client B          Server
 (host)          (joiner)           │
   │                │               │
   │                │── join-room ─►│
   │                │  {roomId,     │
   │                │   playerId}   │
   │                │               ├── RoomManager.joinRoom()
   │                │               │   ├── Validate room exists
   │                │               │   ├── Validate not full
   │                │               │   ├── Create Player O
   │                │               │   ├── Add to room players
   │                │               │   ├── Set status=ACTIVE
   │                │               │   ├── Index player/socket
   │                │               │   └── Return room
   │                │               ├── socket.join(roomId)
   │                │               │
   │                │◄─ room-joined │
   │                │   {roomId,    │
   │                │    player}    │
   │                │               │
   │◄─ player-joined ──────────────►│ (both players)
   │   {players[],symbol}           │
   │                │               │
   │◄─ game-started ──────────────►│ (both players)
   │   {board,                      │
   │    currentPlayer,              │
   │    turnDeadline,               │
   │    playerX, playerO}           │
   │                │               ├── scheduleTurnTimeout()
```

### 5.3 Make Move

```
Client A         Client B         Server
 (current turn)                   │
   │                              │
   │── make-move ────────────────►│
   │  {roomId, playerId,          │
   │   row, col}                  │
   │                              ├── GameEngine.makeMove()
   │                              │   ├── Validate turn, pos, cell
   │                              │   ├── Place symbol on board
   │                              │   ├── Check win/draw
   │                              │   ├── Switch turn
   │                              │   └── Return MoveResult
   │                              ├── clearTurnTimeout()
   │                              │
   │◄─ move-result ──────────────►│ (both players)
   │  {success, board,            │
   │   currentPlayer,             │
   │   turnDeadline,              │
   │   winner?, winningCells?,    │
   │   isDraw, move}              │
   │                              │
   │              (if not game over)│
   │                              ├── scheduleTurnTimeout()
```

### 5.4 Disconnect & Reconnect

```
Client A         Client B         Server
   │                              │
   │── (socket disconnect) ──────►│
   │                              ├── Find room by socketId
   │                              ├── markPlayerDisconnected()
   │                              │
   │◄─ player-disconnected ──────►│ (only B)
   │   {playerId: A.id}           │
   │                              │
   │── (socket reconnects)        │
   │── reconnect ────────────────►│
   │   {playerId: A.id}           │
   │                              ├── updatePlayerSocket()
   │                              ├── markPlayerConnected()
   │                              ├── socket.join(roomId)
   │                              │
   │◄─ room-state ────────────────│ (only A — full state)
   │  {roomId, status,            │
   │   players, board,            │
   │   currentPlayer,             │
   │   turnDeadline, winner,      │
   │   winningCells, isDraw,      │
   │   moves}                     │
   │                              │
   │◄─ player-reconnected ───────►│ (both players)
   │   {playerId: A.id}           │
```

### 5.5 Leave Room

```
Client A         Client B         Server
   │                              │
   │── leave-room ───────────────►│
   │  {roomId, playerId}          │
   │                              ├── GameManager.leaveRoom()
   │                              │   ├── Remove from room.players
   │                              │   ├── Deindex player/socket
   │                              │   ├── If room empty: delete
   │                              │   ├── If was ACTIVE/FINISHED → OPEN
   │                              │   └── Reset game engine
   │                              ├── clearTurnTimeout()
   │                              │
   │◄─ player-left ──────────────►│ (both players — emitted
   │  {playerId, roomId,          │  before socket.leave())
   │   remainingPlayers,          │
   │   roomStatus}                │
   │                              │
   │                              ├── socket.leave(roomId)
```

### 5.6 Play Again

```
Client A         Client B         Server
   │                              │
   │── play-again ───────────────►│
   │  {roomId, playerId}          │
   │                              ├── Validate room FINISHED
   │                              ├── resetGame()
   │                              │   ├── New GameEngine
   │                              │   ├── initialize fresh board
   │                              │   ├── status → ACTIVE
   │                              │   └── Set turn deadline
   │                              ├── clearTurnTimeout()
   │                              │
   │◄─ game-started ─────────────►│ (both players)
   │  {board, currentPlayer,      │
   │   turnDeadline,              │
   │   playerX, playerO}          │
   │                              ├── scheduleTurnTimeout()
```

---

## 6. Socket Event Reference

All events are JSON. No acknowledgement callbacks — listeners must be installed before emitting.

### 6.1 Client → Server

| Event | Payload | Description |
|---|---|---|
| `create-room` | `{ playerId }` | Create an OPEN room, become X |
| `join-room` | `{ roomId, playerId }` | Join an OPEN room, become O |
| `make-move` | `{ roomId, playerId, row, col }` | Place symbol at position |
| `reconnect` | `{ playerId }` | Rebind player to new socket |
| `play-again` | `{ roomId, playerId }` | Request game reset |
| `leave-room` | `{ roomId, playerId }` | Leave room voluntarily |
| `get-rooms` | `{ playerId }` | List OPEN rooms |
| `get-room` | `{ playerId, roomId }` | Get room details + game snapshot |
| `get-health` | `{ playerId }` | Server health check |

### 6.2 Server → Client

| Event | Recipient | Payload |
|---|---|---|
| `room-created` | sender | `{ roomId, player }` |
| `room-joined` | joiner | `{ roomId, player }` |
| `player-joined` | room | `{ playerId, symbol, players[] }` |
| `game-started` | room | `{ board, currentPlayer, turnDeadline, playerX, playerO }` |
| `move-result` | room | `{ success, board, currentPlayer, turnDeadline, winner?, winningCells?, isDraw, move?, timeoutWin?, error? }` |
| `turn-timer` | room | `{ roomId, remainingMs, turnDeadline, currentPlayer }` |
| `player-disconnected` | room | `{ playerId }` |
| `player-reconnected` | room | `{ playerId }` |
| `player-left` | room + leaver | `{ playerId, roomId, remainingPlayers, roomStatus }` |
| `room-state` | reconnecting socket | `{ roomId, status, players[], board, currentPlayer, turnDeadline, winner, winningCells, isDraw, moves }` |
| `rooms-list` | sender | `{ rooms: [{ roomId, status, playerCount, players[], createdAt, updatedAt }] }` |
| `room-details` | sender | `{ roomId, status, players[], game: { board, currentPlayer, turnDeadline, winner, winningCells, isDraw, moves, startedAt, completedAt }, createdAt, updatedAt }` |
| `health-status` | sender | `{ status, server }` |
| `error` | sender | `{ message }` |

---

## 7. REST API

Read-only; gameplay uses Socket.IO.

| Method | Path | Response |
|---|---|---|
| GET | `/health` | `{ status, server }` |
| GET | `/rooms` | `[{ roomId, status, playerCount, players[], createdAt, updatedAt }]` |
| GET | `/rooms/:id` | `{ roomId, status, players[], game: {...}, createdAt, updatedAt }` or `404 { error }` |
| POST | `/rooms` | `501 { error: "Not implemented" }` |

---

## 8. Room Lifecycle

```
        createRoom
            │
            ▼
        ┌──────┐
        │ OPEN │◄───────┐
        └──┬───┘        │
           │            │
      joinRoom      leaveRoom
      (2nd player)  (if was ACTIVE/FINISHED)
           │            │
           ▼            │
        ┌────────┐      │
        │ ACTIVE │──────┘
        └───┬────┘
            │
      makeMove (winner/draw)
            │
            ▼
        ┌──────────┐
        │ FINISHED │───── playAgain ──► ACTIVE
        └──────────┘
            │
       leaveRoom (both players gone)
            │
            ▼
         DELETED
```

---

## 9. Error Handling

### 9.1 Custom Error Classes

**RoomError** hierarchy:
- `RoomNotFoundError` — invalid roomId
- `RoomFullError` — room has 2 players
- `PlayerAlreadyInRoomError` — player in another room
- `DuplicatePlayerError` — same playerId twice
- `DuplicateSocketError` — same socket twice
- `PlayerNotInRoomError` — player not indexed
- `InvalidRoomStateError` — unexpected status

**GameError** hierarchy:
- `GameNotStartedError` — game not initialized
- `GameAlreadyOverError` — game is finished
- `InvalidMoveError` — out-of-bounds
- `NotYourTurnError` — wrong player
- `CellOccupiedError` — cell taken
- `MoveTimeoutError` — turn expired
- `PlayerNotInGameError` — player not in this game

### 9.2 Error Propagation

- Socket handlers catch errors and emit `{ message }` via the `error` event to the requesting socket
- REST errors go through the Express error middleware, returning JSON `{ error }` with appropriate HTTP status
- Move validation errors (invalid cell, wrong turn, etc.) are returned as `move-result { success: false, error }` rather than the `error` event

### 9.3 Edge Cases Handled

| Scenario | Behavior |
|---|---|
| Player creates room while already in one | `error` emitted, room not created |
| Join full/unknown room | `error` emitted, not joined |
| Move on inactive/finished game | `move-result { success: false }` |
| Move out of turn | `move-result { success: false }` |
| Move on occupied cell | `move-result { success: false }` |
| Socket disconnect mid-game | Player marked disconnected, room notified |
| Reconnect after restart | `error: "No active game found"` (state lost) |
| Leave while in ACTIVE game | Room reverts to OPEN, game engine reset |
| Both players leave empty room | Room deleted |
| Play again in non-finished room | `error` emitted |
| Network timeout on turn | Auto-resolve: opponent wins via `timeoutWin` |

---

## 10. Configuration

| Env Variable | Default | Description |
|---|---|---|
| `PORT` | `3000` | HTTP server port |
| `CLIENT_URL` | `http://localhost:5173` | CORS allowed origin |
| `TURN_TIMEOUT` | `30000` | Turn timeout in ms |
| `TURN_TIMEOUT_ENABLED` | `true` | Enable/disable turn timer |

---

## 11. Deployment

### Docker

```bash
docker compose up --build
```

Builds a two-stage Docker image (builder + runner), exposes port 3000, loads `.env`.

### Development

```bash
npm install
npm run dev   # ts-node-dev with hot reload
```

### Production

```bash
npm run build
npm start     # node dist/server.js
```

---

## 12. Limitations & Future Considerations

- **No persistence**: All state in memory; restart loses all rooms. Add Redis or PostgreSQL for persistence.
- **No authentication**: Player IDs are client-generated UUIDs; no auth layer.
- **Single-process**: No horizontal scaling. Use Socket.IO adapter (Redis) for multi-instance.
- **No rate limiting**: Malicious clients could spam events.
- **No logging framework**: Custom `Logger` utility; would benefit from structured logging (e.g., Winston/Pino).
- **No input validation**: Payloads are not schema-validated; relies on TypeScript types at compile time only.
