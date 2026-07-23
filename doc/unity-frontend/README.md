# How to Set Up & Play the Game

## Prerequisites

- **Unity** 6000.045f1 + (with WebGL Build Support module installed)
- **Backend server** running (see `doc/backend-docs/HowToSetUp.md`)

---

## 1. Build the Unity Project for WebGL

1. Open the project in Unity Hub: `LilaAssignment_Unity/`
2. Open Scene: `Assets/ProblemStatement_1/Scenes/TicTacToeScene.unity`
3. Go to **File → Build Settings**
4. Select **TicTacToeWeb** as the Build Profile (click "Switch Platform" if needed)
5. Click **Build And Run**
6. Choose a build folder (e.g. `Builds/WebGL/`)
7. Unity builds the project and opens it in your default browser

## 2. Play the Game

### Step 1: Start the Backend

Make sure the backend server is running on `http://localhost:5000` (see `doc/backend-docs/HowToSetUp.md`).

### Step 2: Open Two Browser Windows

| Player | Action |
|---|---|
| **Player 1** | The build automatically opens in a browser tab |
| **Player 2** | Copy the URL from Player 1's browser tab and open it in a **new window** |

> Each window represents one player. Both must be open simultaneously.

### Step 3: Connect

Wait for the connection status indicator to show **connected** (green). The Lobby UI appears.

### Step 4: Player 1 Creates a Room

1. Click **Create Room** button
2. The room appears in the room list on both windows
3. The UI shows *"waiting for the Player.."* overlay on Player 1's screen

### Step 5: Player 2 Joins

1. In Player 2's window, the room appears in the scrollable room list
2. Click **Join** on that room
3. Both players see *"Joined Room, waiting for Game start.."* briefly
4. Game board appears for both — **Player 1 (X) goes first**

### Step 6: Play Tic-Tac-Toe

- Players take turns clicking an empty tile on the 3×3 grid
- Current player's symbol is shown on the UI
- Turn timer (30s) counts down — if time expires, the current player loses
- Invalid moves (occupied cell, wrong turn) show an error
- Game ends on:
  - **Win** — three-in-a-row highlights the winning line
  - **Draw** — all cells filled, no winner
  - **Timeout** — current player didn't move in time

### Step 7: Play Again or Exit

After the result is displayed (6-second delay on normal win/draw, immediate on timeout):

- **Play Again** — resets the board. Both players keep their symbols (X stays X, O stays O). Play another round in the same session.
- **Exit** — returns both players to the Lobby

### Step 8: Back in Lobby

- Create a new room or join another available room
- Player IDs persist across sessions

---

## Summary of Controls

| UI Element | Action |
|---|---|
| **Create Room** | Host a new game (becomes Player X) |
| **Join** (in room list) | Join an open room (becomes Player O) |
| **Grid Tile** | Make a move (your turn only) |
| **Play Again** | Start a new round in the same room |
| **Exit** | Leave the current room, back to Lobby |
| **New Player ID** | Generate a fresh player identity |

---

## Troubleshooting

| Issue | Fix |
|---|---|
| Connection fails | Ensure backend is running on `http://localhost:5000` |
| Room list empty | Player 1 must click **Create Room** first |
| Moves not registering | Check it's your turn (current player indicator) |
| Timer not counting down | Verify `TURN_TIMEOUT_ENABLED=true` in backend `.env` |
| Both players see same board | Use two **separate browser windows** (not tabs) |
