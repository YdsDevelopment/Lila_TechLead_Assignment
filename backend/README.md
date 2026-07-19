# Tic-Tac-Toe Server

Multiplayer Tic-Tac-Toe game server built with Node.js, Express, Socket.IO, and TypeScript.

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

## Folder Structure

```
src/
├── index.ts              # Barrel exports
├── app.ts                # Express app setup
├── server.ts             # Entry point
├── config/
│   └── env.ts            # Environment configuration
├── controllers/
│   └── RoomController.ts # REST controller
├── routes/
│   ├── health.routes.ts  # Health check endpoint
│   └── room.routes.ts    # Room endpoints
├── socket/
│   ├── socket.ts         # Socket.IO initialization
│   └── handlers/         # Socket event handlers
├── game/
│   ├── GameManager.ts    # Coordinates gameplay
│   ├── RoomManager.ts    # Manages room state
│   ├── GameEngine.ts     # Game rules
│   └── WinnerChecker.ts  # Winner detection
├── models/
│   ├── Player.ts         # Player interface
│   ├── Room.ts           # Room interface
│   ├── Move.ts           # Move interface
│   └── GameState.ts      # GameState interface
├── middleware/
│   ├── errorHandler.ts   # Global error handler
│   └── notFound.ts       # 404 handler
├── utils/
│   ├── Logger.ts         # Logging utility
│   └── RoomIdGenerator.ts# Room ID generation
└── types/
    └── socketEvents.ts   # Socket event constants
```

## Setup

### Prerequisites

- Node.js 20
- npm

### Install dependencies

```bash
npm install
```

### Environment

Copy `.env.example` to `.env`:

```bash
cp .env.example .env
```

### Run locally

```bash
npm run dev
```

Server starts at `http://localhost:3000`.

### Build

```bash
npm run build
```

### Start production

```bash
npm start
```

## Running with Docker

```bash
docker compose up
```

## API Endpoints

| Method | Path         | Description      |
|--------|-------------|------------------|
| GET    | `/health`    | Health check     |
| GET    | `/rooms`     | List rooms       |
| GET    | `/rooms/:id` | Get room by ID   |
| POST   | `/rooms`     | Create a room    |

## Future Development Phases

1. **Room Management** – Create, join, list rooms
2. **Game Logic** – Move validation, turn management, win/draw detection
3. **Socket Events** – Real-time gameplay events
4. **Reconnection** – Handle player disconnections
5. **UI Integration** – Connect with React frontend
6. **Authentication** – Basic player identity
7. **Scaling** – Redis adapter for multi-instance support

## License

MIT
