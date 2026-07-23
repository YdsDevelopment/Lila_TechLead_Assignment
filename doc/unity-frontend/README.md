# How to Set Up & Play the Game

## Prerequisites

| Requirement | Version | Install |
|---|---|---|
| **Unity** | 6000.0.45f1+ (with WebGL module) | Unity Hub |
| **Node.js** | 20+ | See OS-specific section below |
| **Backend server** | — | See `doc/backend-docs/HowToSetUp.md` |

---

## macOS — Setup

### 1. Install Node.js

```bash
# Using Homebrew (recommended)
brew install node@20

# Or using nvm (version manager)
curl -o- https://raw.githubusercontent.com/nvm-sh/nvm/v0.40.1/install.sh | bash
nvm install 20
```

### 2. Install Vite

```bash
npm install -g vite
```

Or skip this and use `npx vite` later — it auto-downloads on first run.

### 3. Build the Unity Project for WebGL

1. Open the project in Unity Hub: `LilaAssignment_Unity/`
2. Open Scene: `Assets/ProblemStatement_1/Scenes/TicTacToeScene.unity`
3. Go to **File → Build Profiles**
4. Select **TicTacToeWeb** profile and press Switch platform if needed
7. Close Player Settings
8. Click **Build** (not Build And Run)
9. Choose a build folder (e.g. `Builds/WebGL/`)
10. Unity outputs uncompressed files: `index.html`, `Build/*.data`, `Build/*.wasm`, `Build/*.js`

### 4. Serve the Build with Vite

```bash
cd "/path/to/your/Builds/WebGL/"
npx vite --host 0.0.0.0 --port 7000
```

Open: **http://localhost:7000**

---

## Windows — Setup

### 1. Install Node.js

```powershell
# Using winget (recommended)
winget install OpenJS.NodeJS.LTS

# Or download from https://nodejs.org (LTS v20+)
```

### 2. Install Vite

```powershell
npm install -g vite
```

Or skip this and use `npx vite` later — it auto-downloads on first run.

### 3. Build the Unity Project for WebGL

1. Open the project in Unity Hub: `LilaAssignment_Unity/`
2. Open Scene: `Assets/ProblemStatement_1/Scenes/TicTacToeScene.unity`
3. Go to **File → Build Profiles**
4. Select **TicTacToeWeb** profile and press Switch platform if needed
7. Close Player Settings
8. Click **Build** (not Build And Run)
9. Choose a build folder (e.g. `Builds/WebGL/`)
10. Unity outputs uncompressed files: `index.html`, `Build/*.data`, `Build/*.wasm`, `Build/*.js`

### 4. Serve the Build with Vite

```powershell
cd "\path\to\your\Builds\WebGL\"
npx vite --host 0.0.0.0 --port 7000
```

Open: **http://localhost:7000**

---

## Backend

Start the backend server (see `doc/backend-docs/HowToSetUp.md`).

The backend `.env` must allow connections from the Vite server:

```env
PORT=5000
CLIENT_URL=http://localhost:7000
```

---

## Play the Game

### Step 1: Open Two Browser Windows

| Player | URL |
|---|---|
| **Player 1** | `http://localhost:7000` |
| **Player 2** | `http://localhost:7000` (new window) |

> Use two **separate browser windows** (not tabs in the same window). Each window acts as an independent player.

### Step 2: Connect

Wait for the connection status indicator to show **connected** (green). The Lobby UI appears on both windows.

### Step 3: Player 1 Creates a Room

1. Click **Create Room** button
2. The room appears in the room list on both windows
3. Player 1 sees *"waiting for the Player.."* overlay

### Step 4: Player 2 Joins

1. In Player 2's window, the new room is visible in the scrollable list
2. Click **Join** on that room
3. Both players see *"Joined Room, waiting for Game start.."* briefly
4. The game board appears — **Player 1 (X) goes first**

### Step 5: Play Tic-Tac-Toe

- Players take turns clicking an empty tile on the 3×3 grid
- Current player's symbol is highlighted on the UI
- Turn timer (30s) counts down — if time expires, the current player loses
- Game ends on:
  - **Win** — three-in-a-row highlights the winning line
  - **Draw** — all 9 cells filled, no winner
  - **Timeout** — current player didn't move within the time limit

### Step 6: Play Again or Exit

After the result (6-second delay on win/draw, immediate on timeout):

- **Play Again** — resets the board, same room, same session. X stays X, O stays O. Play another round.
- **Exit** — returns to the Lobby

### Step 7: Back in Lobby

- Create a new room or join an available one
- Player IDs persist in browser storage across sessions

---

## Summary of Controls

| UI Element | Action |
|---|---|
| **Create Room** | Host a new game (you are Player X) |
| **Join** (in room list) | Join an open room (you are Player O) |
| **Grid Tile** | Make a move (your turn only) |
| **Play Again** | Start a new round in the same room |
| **Exit** | Leave the room, back to Lobby |
| **New Player ID** | Generate a fresh player identity |

---

## Troubleshooting

| Issue | Fix |
|---|---|
| Connection fails | Backend running? `.env` has `CLIENT_URL=http://localhost:7000`? |
| `.data` file 404 on load | Build was compressed — rebuild with **Compression Format = Disabled** |
| Room list empty | Player 1 must click **Create Room** first |
| Moves not registering | Check current player indicator — is it your turn? |
| Timer not counting down | Verify `TURN_TIMEOUT_ENABLED=true` in backend `.env` |
| Both players see same game state | Use two **separate browser windows** |
| Reconnect not working | Server restart clears all in-memory state — start fresh |
