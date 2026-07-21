# Tic-Tac-Toe Multiplayer Server

A real-time multiplayer Tic-Tac-Toe game server built with **Node.js**, **Express**, **Socket.IO**, and **TypeScript**.

## Architecture

```
                    React Client
                         |
             REST + Socket.IO
                         |
                  Express Server
                         |
                  Socket.IO Server
                         |
                 Socket Event Layer
                         |
                   GameManager
                 /             \
          RoomManager      GameEngine
                                |
                        WinnerChecker
```

## Implemented Features

### Game Engine (`src/game/`)
- **GameEngine**: Full Tic-Tac-Toe logic — 3×3 board, move validation (out-of-bounds, cell occupied, wrong turn, timeout), win/draw detection, turn deadline enforcement.
- **WinnerChecker**: Pure function checking all 8 winning lines (3 rows, 3 cols, 2 diagonals); returns winner symbol, winning cells, and draw status.
- **GameError Hierarchy**: 7 domain-specific errors (`GameNotStartedError`, `GameAlreadyOverError`, `InvalidMoveError`, `NotYourTurnError`, `CellOccupiedError`, `MoveTimeoutError`, `PlayerNotInGameError`).

### Room / Lobby (`src/game/`)
- **RoomManager**: In-memory room CRUD with fast lookup indices (`playerId → room`, `socketId → room`). Supports create, join, leave, delete, query open/active rooms, player disconnect/reconnect marking.
- **Room Lifecycle**: Rooms transition through `OPEN → ACTIVE → FINISHED`.
- **RoomError Hierarchy**: 7 domain-specific errors (`RoomNotFoundError`, `RoomFullError`, `PlayerAlreadyInRoomError`, `DuplicatePlayerError`, `DuplicateSocketError`, `InvalidRoomStateError`, `PlayerNotInRoomError`).
- **Auto-Game-Start**: Game engine initializes automatically when the second player joins.
- **Room ID Generation**: UUID v4-based room IDs.

### Real-Time Multiplayer via Socket.IO (`src/socket/`)
| Event | Direction | Description |
|---|---|---|
| `create-room` | Client → Server | Host creates a room as Player X |
| `room-created` | Server → Client | Returns room ID and host summary |
| `join-room` | Client → Server | Second player joins as Player O |
| `room-joined` | Server → Client | Confirms join to the joiner |
| `player-joined` | Server → Room | Notifies room of new player |
| `game-started` | Server → Room | Full board, players, deadlines |
| `make-move` | Client → Server | Submit a move (row, col) |
| `move-result` | Server → Room | Broadcasts updated board, winner, turn |
| `disconnect` | Native → Server | Marks player disconnected, notifies room |
| `player-disconnected` | Server → Room | Alerts room of disconnection |
| `reconnect` | Client → Server | Player reconnects with player ID |
| `room-state` | Server → Client | Full game snapshot on reconnect |
| `player-reconnected` | Server → Room | Alerts room of reconnection |
| `turn-timer` | Server → Room | Countdown sync — `remainingMs` sent every 1s |

### REST API (`src/routes/`, `src/controllers/`)
| Method | Path | Status |
|---|---|---|
| `GET` | `/health` | ✅ Implemented — returns `{ status: "ok" }` |
| `GET` | `/rooms` | ⏳ Stub — 501 Not Implemented |
| `GET` | `/rooms/:id` | ⏳ Stub — 501 Not Implemented |
| `POST` | `/rooms` | ⏳ Stub — 501 Not Implemented |

### Infrastructure (`src/config/`, `src/middleware/`, `src/utils/`)
- **Express App**: CORS config, JSON body parsing, modular route mounting.
- **Error Handling**: Global 500 error handler + 404 catch-all middleware.
- **Environment Config**: `dotenv`-based — port (default 3000), client URL, turn timeout (default 30s).
- **Logger**: Timestamped console logger with `info`, `warn`, `error` levels.
- **Barrel Exports**: Central `index.ts` re-exports all public API.

### Socket Event Types (`src/types/`)
- `SocketEvents` — 15 event name constants.
- `ServerEvents` / `ClientEvents` — Fully typed payload interfaces for every event.
- Type-safe payloads: `CreateRoomPayload`, `GameStartedPayload`, `MoveResultPayload`, `RoomStatePayload`, `ErrorPayload`, etc.

## Setup

### Prerequisites
- Node.js 20+
- npm

### Install & Run
```bash
cd backend
npm install
npm run dev
```

Server starts at `http://localhost:3000`.

### Build for Production
```bash
npm run build
npm start
```

### Docker
```bash
docker compose up
```

## Project Structure

```
backend/
├── src/
│   ├── server.ts             # Entry point — HTTP + Socket.IO bootstrap
│   ├── app.ts                # Express app setup
│   ├── index.ts              # Barrel exports
│   ├── config/
│   │   └── env.ts            # Environment config
│   ├── controllers/
│   │   └── RoomController.ts # REST controller (stubs)
│   ├── routes/
│   │   ├── health.routes.ts  # Health check endpoint
│   │   └── room.routes.ts    # Room endpoints (stubs)
│   ├── socket/
│   │   ├── socket.ts         # Socket.IO init + connection handler
│   │   └── handlers/         # Event handlers
│   │       ├── createRoom.handler.ts
│   │       ├── joinRoom.handler.ts
│   │       ├── makeMove.handler.ts
│   │       ├── disconnect.handler.ts
│   │       └── reconnect.handler.ts
│   ├── game/
│   │   ├── GameManager.ts    # Facade coordinating Room + Game
│   │   ├── RoomManager.ts    # Room CRUD + player management
│   │   ├── GameEngine.ts     # Core game rules engine
│   │   ├── WinnerChecker.ts  # Win/draw detection
│   │   ├── RoomError.ts      # Room error types
│   │   └── GameError.ts      # Game error types
│   ├── models/
│   │   ├── Player.ts
│   │   ├── Room.ts
│   │   ├── RoomStatus.ts
│   │   ├── Move.ts
│   │   └── GameState.ts
│   ├── types/
│   │   ├── socketEvents.ts   # Event name constants
│   │   └── gameEvents.ts     # Event payload types
│   ├── middleware/
│   │   ├── errorHandler.ts   # Global error handler
│   │   └── notFound.ts       # 404 handler
│   └── utils/
│       ├── Logger.ts         # Logging utility
│       └── RoomIdGenerator.ts# UUID room ID generator
├── package.json
├── tsconfig.json
├── Dockerfile
├── docker-compose.yml
└── .env.example
```

## Test Coverage

No test files are currently present in the project.

## Turn Switching

Player turn switching is handled in **`backend/src/game/GameEngine.ts:157`** within the `makeMove` method. After a valid move is made and no win/draw is detected, the engine toggles `currentPlayerIndex` between `0` and `1`:

```typescript
this.currentPlayerIndex = this.currentPlayerIndex === 0 ? 1 : 0;
this.turnDeadline = Date.now() + this.turnTimeoutMs;
```

The move handler in **`backend/src/socket/handlers/makeMove.handler.ts`** then broadcasts the `move-result` event with the updated `currentPlayer` and `turnDeadline` to all players in the room.

## Turn Timeout / Sync Timer

The turn timeout feature automatically declares the opponent as the winner if the active player fails to make a move before the deadline.

### Configuration (`backend/src/config/env.ts`)
- **`TURN_TIMEOUT`** — Duration in milliseconds (default: `30000`).
- **`TURN_TIMEOUT_ENABLED`** — Toggle to enable/disable the timeout feature (default: `true`). Set to `false` to disable auto-loss on timeout.

### Backend Implementation
| File | Role |
|---|---|
| `backend/src/config/env.ts:9` | Reads `TURN_TIMEOUT_ENABLED` env var (`true` by default) |
| `backend/src/game/GameEngine.ts:62` | `initialize()` accepts `turnTimeoutEnabled` param |
| `backend/src/game/GameEngine.ts:108` | `makeMove()` — if timeout enabled & deadline passed, calls `resolveTimeout()` instead of throwing |
| `backend/src/game/GameEngine.ts:178` | `checkTimeout()` — returns `MoveResult` if the active player exceeded the deadline |
| `backend/src/game/GameEngine.ts:246` | `resolveTimeout()` — sets opponent as winner, ends game |
| `backend/src/game/GameEngine.ts:191` | `isTurnTimeoutEnabled()` — getter for the toggle flag |
| `backend/src/game/GameManager.ts:43` | `checkTimeout(roomId)` — facade that calls engine and marks room `FINISHED` |
| `backend/src/socket/socket.ts:40` | `scheduleTurnTimeout()` — per-room `setTimeout`; broadcasts `move-result` with `timeoutWin: true` |
| `backend/src/socket/socket.ts:69` | `clearTurnTimeout()` — cancels pending timeout for a room |
| `backend/src/socket/handlers/makeMove.handler.ts:72` | After successful move, schedules next turn timeout; clears on game end |
| `backend/src/socket/handlers/joinRoom.handler.ts:69` | Schedules first turn timeout when game starts |

### Flow
1. Game starts → `joinRoom.handler.ts` schedules a timeout for the first player's turn via `scheduleTurnTimeout()`.
2. Player makes a move → `makeMove.handler.ts` cancels old timeout (`clearTurnTimeout()`) and schedules a new one for the next player.
3. If the timeout fires → `GameEngine.checkTimeout()` marks the opponent as winner → `move-result` broadcast with `timeoutWin: true`, `currentPlayer: null`, and opponent as `winner`.
4. If the active player attempts a move after deadline with timeout enabled → `makeMove()` calls `resolveTimeout()` directly, opponent wins immediately.

### Server → Client Data Flow
| Event | Fields sent to client |
|---|---|
| `game-started` | `board`, `currentPlayer`, `turnDeadline` (absolute epoch ms), `playerX`, `playerO` |
| `move-result` | `board`, `currentPlayer`, `turnDeadline`, `winner`, `winningCells`, `isDraw`, `timeoutWin` |
| `turn-timer` (every 1s) | `roomId`, `remainingMs`, `turnDeadline`, `currentPlayer` |
| `room-state` | `board`, `currentPlayer`, `turnDeadline`, `winner`, `winningCells`, `isDraw`, `players` |

The server runs a 1-second `setInterval` (`socket.ts:startTimerBroadcast`) that iterates all active rooms, computes `remainingMs = turnDeadline - Date.now()`, and emits `turn-timer` to each room. The frontend receives `remainingMs` directly — no local clock-based computation.

### Server-Side Timer Broadcast (`backend/src/socket/socket.ts`)
- **`startTimerBroadcast()`** (line 40) — starts a `setInterval` at 1s that:
  1. Fetches all active rooms via `GameManager.getActiveRooms()`
  2. For each room with an active game and valid `turnDeadline`, computes `remainingMs`
  3. Emits `turn-timer` event with `{ roomId, remainingMs, turnDeadline, currentPlayer }` to the room
- Runs alongside the existing timeout-based auto-loss system (`scheduleTurnTimeout` / `clearTurnTimeout`)

### Frontend (`frontend/index.html`)
- **Countdown display** — `#infoTurnTimer` shows seconds with 1 decimal (e.g. `24.3s`), updated every 1s from server.
- **Progress bar** — `#turnTimerBar` / `#turnTimerFill` shrinks based on `remainingMs / total` ratio; green → yellow at <40% → red pulsing at <20%.
- **Timer sync** — Listens for `turn-timer` event and calls `updateTimerDisplay(remainingMs)`.
- **On timeout** — displays "Opponent timed out — You win!/You lost!" in game status.
- **Cleanup** — timer hidden when game ends (win/draw) or on disconnect via `hideTimerDisplay()`.
- **Timer functions**:
  - `updateTimerDisplay(remainingMs)` — updates the display text and progress bar from the server-supplied value.
  - `resetTurnTimer(data)` — stores `turnDeadline` from game events to compute the progress bar total; calls `hideTimerDisplay()` if no active game.
  - `hideTimerDisplay()` — clears state and hides the timer bar.

### Socket Events
| Event | Direction | Description |
|---|---|---|
| `turn-timer` | Server → Room | Broadcasts remaining turn time every 1s |

## License

MIT
