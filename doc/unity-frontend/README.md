# How to Set Up & Play the Game

## Prerequisites

| Requirement | Version | Install |
|---|---|---|
| **Unity** | 6000.0.45f1+ (with WebGL module) | Unity Hub |
| **Node.js** | 20+ | See OS-specific section below |
| **Backend server** | — | See `doc/backend-docs/HowToSetUp.md` |

---

## macOS

1. Install Node.js (if not Installed)
   Open terminal and run `brew install node@20` (install using HomeBrew)

2. Install Vite
   run `npm install -g vite`

3. Go to the Client(Game) build Folder in terminal
   `cd /LilaAssignment_PavanChavan/ClientBuild`

4. run `npx vite --host 0.0.0.0 --port 7000`

5. This will starts the Client at `http://localhost:7000`.


## Windows
1. Install Node.js (if not Installed)
   open command prompt as Administrator
   run `winget install OpenJS.NodeJS.LTS`

2. Install Vite
   run `npm install -g vite`

3. Go to the Client(Game) build Folder in terminal
   `cd /LilaAssignment_PavanChavan/ClientBuild`

4. run `npx vite --host 0.0.0.0 --port 7000`

5. This will starts the Client at `http://localhost:7000`.


## HOW TO Play the Game

1. Open Two Browser Windows
   open `http://localhost:7000` link in Both Browser Windows

2. Wait for the connection status indicator to show `connected` (green). The Lobby UI appears on both windows.

3. Player 1 Creates a Room
  1. Click `Create Room` button
  2. The room appears in the room list on both windows
  3. Player 1 sees `waiting for the Player..` overlay

4. Player 2 Joins
  1. In Player 2's window, the new room is visible in the Room list
  2. Click `Join` on that room
  3. Both players will see the Game Board

5. Play Tic-Tac-Toe

  - Players take turns clicking an empty tile on the 3×3 grid
  - Current player's symbol is highlighted on the UI
  - Turn timer (30s) counts down — if time expires, the current player loses

  - Game ends on:
    - `Win` — three-in-a-row highlights the winning line
    - `Draw` — all 9 cells filled, no winner
    - `Timeout` — current player didn't move within the time limit

6. Play Again or Exit

  After the result (6-second delay on win/draw, immediate on timeout):

  - `Play Again` — resets the board, same room, same session. X stays X, O stays O. Play another round.
  - `Exit` — returns to the Lobby

7. Back in Lobby

  - Create a new room or join an available one
  - Player IDs persist in browser storage across sessions
