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
- `SocketEvents` — 14 event name constants.
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

## License

MIT
