# Unity WebGL Tic-Tac-Toe — Socket.IO Client Implementation Plan

This directory contains C# scripts for integrating the backend Tic-Tac-Toe multiplayer server with a **Unity WebGL** game client using Socket.IO.

WebGL builds run in the browser and have specific constraints: WebSocket-only transport, no threading, CORS requirements, and browser lifecycle events. This plan addresses each of those.

---

## Architecture

```
Browser Tab (Unity WebGL)
       │
       ▼
Unity MonoBehaviours
       │
       ▼
TicTacToeSocketClient  (high-level API)
       │
       ▼
 ISocketManager         (interface)
       │
       ▼
 SocketManager          (SocketIOClient for Unity)
       │
       ▼
 SocketIOUnity          (WebGL-compatible)
       │
       ▼
 WebSocket (wss://)     ← Transport limited to WebSocket in WebGL
       │
       ▼
Backend Server          (Node.js + Socket.IO)
```

---

## WebGL-Specific Considerations

### 1. Socket Transport

WebGL supports **only WebSocket transport**. The `SocketManager` is already configured with:

```csharp
Transport = SocketIOClient.Transport.TransportProtocol.WebSocket
```

This forces WebSocket-only mode. The HTTP long-polling fallback does not work in WebGL and must be disabled.

### 2. Library Choice

Use the **`SocketIOClient`** NuGet package by doghappy (`https://github.com/doghappy/socket.io-client-csharp`). The `SocketIOUnity` class in this package is designed for Unity and works on all platforms including WebGL.

**Installation for WebGL:**
1. Download the `.unitypackage` or DLLs from the releases page (look for `SocketIOClient.Unity` or the main package).
2. Import into `Assets/Plugins/`.
3. The DLLs must be enabled for the **WebGL** platform in the Unity Plugin Inspector.

Alternatively, install via **Unity Package Manager** → Add package from git URL:
```
https://github.com/doghappy/socket.io-client-csharp.git?path=src/SocketIOClient.Unity/Assets/SocketIOClient.Unity
```

### 3. CORS Configuration

The backend already has CORS configured via the `CLIENT_URL` environment variable (default `http://localhost:5173`).

For WebGL deployments:
- **Local development**: Host the WebGL build on `http://localhost:5173` (use `npx serve` or Unity's built-in host) and set `CLIENT_URL=http://localhost:5173` on the backend (already the default).
- **Production**: Set `CLIENT_URL` to the URL where the WebGL build is hosted (e.g. `https://mygame.example.com`).

The backend `env.ts`:
```typescript
clientUrl: process.env.CLIENT_URL || "http://localhost:5173",
```

### 4. Unity Main Thread

Socket.IO callbacks arrive on a background thread in WebGL. The `SocketManager` must marshal events to the Unity main thread before invoking MonoBehaviours.

The `SocketManager` uses `UnityMainThreadDispatcher` — a simple MonoBehaviour that queues actions and executes them on `Update()`:

```csharp
// Add this to the NetworkManager or a dedicated GameObject
public class UnityMainThreadDispatcher : MonoBehaviour
{
    private static readonly Queue<Action> _queue = new Queue<Action>();
    private static readonly object _lock = new object();

    public static void Enqueue(Action action)
    {
        lock (_lock) _queue.Enqueue(action);
    }

    private void Update()
    {
        lock (_lock)
        {
            while (_queue.Count > 0)
                _queue.Dequeue()?.Invoke();
        }
    }
}
```

The `SocketManager` should dispatch all event invocations through this dispatcher:

```csharp
private void RegisterHandler<T>(string eventName, Action<T> handler)
{
    _socket.On(eventName, response =>
    {
        UnityMainThreadDispatcher.Enqueue(() =>
        {
            try
            {
                string rawJson = response.ToString();
                T payload = JsonHelper.Deserialize<T>(rawJson);
                handler(payload);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[SocketManager] Failed to deserialize '{eventName}': {ex.Message}");
            }
        });
    });
}
```

### 5. Browser Lifecycle

`OnApplicationQuit()` does **not** reliably fire in WebGL when the user closes the tab. Instead:

- **`OnApplicationFocus(bool)`** — fires when the tab gains/loses focus. Use this to show "Reconnecting..." when focus returns.
- **`OnBeforeUnload`** — use a JavaScript bridge (via `jslib`) to notify the server when the tab closes.
- The server already handles abrupt disconnects: when the WebSocket drops, the `disconnect` handler fires and the server cleans up the room.

```csharp
private void OnApplicationFocus(bool hasFocus)
{
    if (!hasFocus && _client.IsConnected)
    {
        // Tab lost focus — optionally show a warning
    }
    else if (hasFocus && !_client.IsConnected && _wasConnectedBefore)
    {
        // Tab regained focus but connection dropped — reconnect
        ShowReconnectingOverlay();
        _client.Connect(_serverUrl, _playerId);
    }
}
```

For graceful disconnect on tab close, add a `.jslib` file:

```javascript
// Assets/Plugins/WebGL/WebGLPlugin.jslib
mergeInto(LibraryManager.library, {
    OnBeforeUnload: function () {
        window.addEventListener("beforeunload", function () {
            // The WebSocket will close automatically when the tab closes.
            // The server detects this via the 'disconnect' event.
        });
    }
});
```

### 6. PlayerPrefs in WebGL

`PlayerPrefs` works in WebGL — data is stored in the browser's IndexedDB. This is ideal for persisting the `playerId` across sessions:

```csharp
private string GetOrCreatePlayerId()
{
    string id = PlayerPrefs.GetString("playerId", "");
    if (string.IsNullOrEmpty(id))
    {
        id = System.Guid.NewGuid().ToString();
        PlayerPrefs.SetString("playerId", id);
        PlayerPrefs.Save();
    }
    return id;
}
```

Note: `PlayerPrefs.Save()` is required in WebGL (it's called automatically on app quit in standalone, but WebGL may not trigger this reliably).

### 7. Build Settings

| Setting | Value | Reason |
|---|---|---|
| `Player Settings → WebGL → Compression` | Disabled or Gzip | Brotli may cause issues with some servers |
| `Player Settings → WebGL → Enable Exceptions` | Full (development) / Explicit Throws Only (production) | Helps debug socket errors |
| `Player Settings → Resolution → Default Canvas Size` | 960×600 or appropriate | Match your game layout |
| `Player Settings → Publishing → Enable Debugging` | Checked (development) | Debug logs in browser console |
| `Build Settings → WebGL → Development Build` | Checked (development) | Unminified code for debugging |

---

## Files

| File | Description |
|---|---|
| `Enums.cs` | `PlayerSymbol` (X, O) with `[JsonConverter(typeof(StringEnumConverter))]` and `RoomStatus` (OPEN, ACTIVE, FINISHED) |
| `Models.cs` | `Player`, `Room`, `Move`, `GameState`, `PlayerSummary`, `MoveSummary` |
| `Payloads.cs` | All 17 request/response payloads matching the backend types |
| `SocketEventNames.cs` | String constants for all 17 socket event names |
| `ISocketManager.cs` | Interface for socket lifecycle + event subscriptions |
| `SocketManager.cs` | SocketIOClient implementation — WebGL-safe (WebSocket-only transport, main thread dispatch via `UnityMainThreadDispatcher`) |
| `JsonHelper.cs` | Centralized JSON serializer/deserializer (camelCase + StringEnum, used by SocketManager) |
| `TicTacToeSocketClient.cs` | High-level client wrapping the socket manager — tracks `_roomId`, `_mySymbol`, exposes typed events to MonoBehaviours |

---

## Dependencies

### SocketIOClient (NuGet)

```
https://github.com/doghappy/socket.io-client-csharp
```

The package provides `SocketIOUnity` which works on all Unity platforms including WebGL.

**Import methods:**

1. **Unity Package Manager (UPM)** — Add via git URL:
   ```
   https://github.com/doghappy/socket.io-client-csharp.git?path=src/SocketIOClient.Unity/Assets/SocketIOClient.Unity
   ```

2. **Manual DLL** — Download the release, copy `SocketIOClient.dll` and `Newtonsoft.Json.dll` into `Assets/Plugins/`. Ensure they are enabled for WebGL in the Plugin Inspector.

### Newtonsoft.Json

Included with SocketIOClient. If installing separately, use the `Newtonsoft.Json-for-Unity` package from the Asset Store or UPM.

---

## Implementation Plan

### Phase 0: Project Setup (WebGL)

1. Create a new Unity project, switch platform to **WebGL** (`File → Build Settings → WebGL → Switch Platform`).
2. Install `SocketIOClient` via UPM or manual DLL (see Dependencies above).
3. Create `UnityMainThreadDispatcher.cs` — a singleton MonoBehaviour that queues actions from socket threads and executes them on `Update()`.
4. Copy all scripts from `UnityOpenCodeScripts/Scripts/` into `Assets/Scripts/`.
5. Ensure all DLLs are enabled for WebGL (Plugin Inspector → WebGL checkbox).
6. Create `NetworkManager.cs` — the singleton entry point (code below).

### Phase 1: UnityMainThreadDispatcher

```csharp
using System;
using System.Collections.Generic;
using UnityEngine;

public class UnityMainThreadDispatcher : MonoBehaviour
{
    private static UnityMainThreadDispatcher _instance;
    private readonly Queue<Action> _queue = new Queue<Action>();
    private readonly object _lock = new object();

    public static UnityMainThreadDispatcher Instance
    {
        get
        {
            if (_instance == null)
            {
                var go = new GameObject("[MainThreadDispatcher]");
                _instance = go.AddComponent<UnityMainThreadDispatcher>();
                DontDestroyOnLoad(go);
            }
            return _instance;
        }
    }

    public void Enqueue(Action action)
    {
        lock (_lock) _queue.Enqueue(action);
    }

    private void Update()
    {
        lock (_lock)
        {
            while (_queue.Count > 0)
                _queue.Dequeue()?.Invoke();
        }
    }
}
```

### Phase 2: Network Manager (WebGL)

```csharp
using UnityEngine;
using TicTacToe;

public class NetworkManager : MonoBehaviour
{
    public static NetworkManager Instance { get; private set; }

    private TicTacToeSocketClient _client;
    public TicTacToeSocketClient Client => _client;

    [SerializeField] private string _serverUrl = "http://localhost:3000";
    private string _playerId;
    private bool _wasConnectedBefore;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Ensure main thread dispatcher exists before socket connects
        _ = UnityMainThreadDispatcher.Instance;

        _playerId = GetOrCreatePlayerId();
        var socketManager = new SocketManager();
        _client = new TicTacToeSocketClient(socketManager);
        RegisterClientEvents();
    }

    private void Start()
    {
        // Auto-connect on startup (optional)
        Connect();
    }

    private string GetOrCreatePlayerId()
    {
        string id = PlayerPrefs.GetString("playerId", "");
        if (string.IsNullOrEmpty(id))
        {
            id = System.Guid.NewGuid().ToString();
            PlayerPrefs.SetString("playerId", id);
            PlayerPrefs.Save();
        }
        return id;
    }

    public void Connect()
    {
        _client.Connect(_serverUrl, _playerId);
    }

    public void DisconnectAndCleanup()
    {
        if (_client.IsInRoom)
            _client.LeaveRoom();
        _client.Disconnect();
    }

    private void RegisterClientEvents()
    {
        _client.OnConnected += () =>
        {
            Debug.Log("[NetworkManager] Connected");
            _wasConnectedBefore = true;
            if (_client.IsInRoom)
                _client.Reconnect(); // Request state restore on reconnect
        };

        _client.OnDisconnected += (reason) =>
        {
            Debug.Log($"[NetworkManager] Disconnected: {reason}");
            ShowReconnectingOverlay();
        };

        _client.OnError += (msg) => { /* Show error popup */ };

        _client.OnRoomCreated += (roomId, player) =>
        {
            Debug.Log($"[NetworkManager] Room created: {roomId}");
            // Show room ID for sharing
        };

        _client.OnRoomJoined += (roomId, player) =>
        {
            Debug.Log($"[NetworkManager] Room joined: {roomId}");
        };

        _client.OnPlayerJoined += (payload) =>
        {
            // Update player list
        };

        _client.OnPlayerLeft += (payload) =>
        {
            if (payload.playerId == _client.PlayerId)
            {
                ReturnToLobby();
            }
            else
            {
                ShowWaitingForOpponent();
            }
        };

        _client.OnGameStarted += () =>
        {
            Debug.Log("[NetworkManager] Game started");
            EnableGameBoard(true);
        };

        _client.OnMoveResult += (payload) =>
        {
            HandleMoveResult(payload);
        };

        _client.OnTurnTimer += (remainingMs, deadline) =>
        {
            UpdateTurnTimer(remainingMs);
        };

        _client.OnPlayerDisconnected += (playerId) =>
        {
            ShowPlayerDisconnected(playerId);
        };

        _client.OnPlayerReconnected += (playerId) =>
        {
            HidePlayerDisconnected(playerId);
        };

        _client.OnRoomState += (payload) =>
        {
            HideReconnectingOverlay();
            // Restore full game state from payload
            RestoreGameState(payload);
        };
    }

    // --- Browser lifecycle ---
    private void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus && _client.IsConnected)
        {
            // Tab blurred — optional pause/warning
        }
        else if (hasFocus && !_client.IsConnected && _wasConnectedBefore)
        {
            ShowReconnectingOverlay();
            Connect();
        }
    }

    private void OnApplicationQuit()
    {
        DisconnectAndCleanup();
    }
}
```

### Phase 3: UI Scenes (WebGL)

#### Lobby Scene
- Player ID (auto-generated, persisted in PlayerPrefs) — read-only display
- Server URL input (default `http://localhost:3000`)
- **Connect** button → `NetworkManager.Instance.Connect()`
- Connection status indicator (green dot / red dot + text)
- **Create Room** → `Client.CreateRoom()` → show returned Room ID prominently
- **Join Room** → text input for Room ID → `Client.JoinRoom(roomId)`
- Room list (optional: REST call to `GET /rooms` — note: WebGL CORS applies)
- **Note**: The server URL in WebGL must be an absolute URL (`http://localhost:3000` or `https://yourserver.com`). Relative URLs may not resolve correctly depending on hosting.

#### Game Scene
- 3×3 grid of clickable UI buttons (arranged in a GridLayoutGroup or manual layout)
- Player info: "You are X" / "You are O", opponent indicator
- Turn timer bar (slider) — receives `remainingMs` every 1s from server
- Turn indicator: "Your Turn" / "Opponent's Turn" — updated from `payload.currentPlayer`
- Win/Draw overlay: shows result, highlights winning cells
- **Play Again** button — enabled only after game ends (winner, draw, or timeout)
- **Exit** button — returns to lobby, calls `Client.LeaveRoom()`

### Phase 4: Board Rendering (WebGL)

Use `Client.NullableBoardFromPayload(payload.board)` to convert server data to `PlayerSymbol?[,]`:

```csharp
public class BoardRenderer : MonoBehaviour
{
    [SerializeField] private UnityEngine.UI.Button[,] cells; // 3x3 grid

    public void UpdateBoard(PlayerSymbol?[,] board)
    {
        for (int r = 0; r < 3; r++)
        {
            for (int c = 0; c < 3; c++)
            {
                var cell = cells[r, c];
                var symbol = board[r, c];
                var text = cell.GetComponentInChildren<UnityEngine.UI.Text>();

                if (symbol == null)
                {
                    text.text = "";
                    cell.interactable = true;
                    cell.onClick.RemoveAllListeners();
                    int row = r, col = c;
                    cell.onClick.AddListener(() => MakeMove(row, col));
                }
                else
                {
                    text.text = symbol == PlayerSymbol.X ? "X" : "O";
                    cell.interactable = false;
                    cell.onClick.RemoveAllListeners();
                }
            }
        }
    }

    private void MakeMove(int row, int col)
    {
        NetworkManager.Instance.Client.MakeMove(row, col);
        SetInteractable(false); // Disable until server confirms
    }

    public void HighlightWinningCells(List<List<int>> winningCells)
    {
        if (winningCells == null) return;
        foreach (var cell in winningCells)
        {
            var img = cells[cell[0], cell[1]].GetComponent<UnityEngine.UI.Image>();
            img.color = Color.green;
        }
    }

    public void SetInteractable(bool active)
    {
        foreach (var cell in cells)
            cell.interactable = active;
    }

    public void Reset()
    {
        foreach (var cell in cells)
        {
            cell.GetComponentInChildren<UnityEngine.UI.Text>().text = "";
            cell.GetComponent<UnityEngine.UI.Image>().color = Color.white;
            cell.interactable = false;
            cell.onClick.RemoveAllListeners();
        }
    }
}
```

### Phase 5: Timer Display (WebGL)

```csharp
public class TurnTimerUI : MonoBehaviour
{
    [SerializeField] private UnityEngine.UI.Slider timerSlider;
    [SerializeField] private UnityEngine.UI.Text timerText;
    [SerializeField] private UnityEngine.UI.Image timerFill; // For color changes

    private long _totalDuration;

    private void Start()
    {
        NetworkManager.Instance.Client.OnTurnTimer += OnTurnTimer;
        NetworkManager.Instance.Client.OnGameStarted += Reset;
    }

    private void OnTurnTimer(long remainingMs, long turnDeadline)
    {
        if (_totalDuration == 0)
            _totalDuration = remainingMs;

        float pct = _totalDuration > 0 ? Mathf.Clamp01((float)remainingMs / _totalDuration) : 0;
        timerSlider.value = pct;
        timerText.text = $"{(remainingMs / 1000f):F1}s";

        if (timerFill != null)
        {
            if (pct < 0.2f)
                timerFill.color = Color.red;
            else if (pct < 0.4f)
                timerFill.color = Color.yellow;
            else
                timerFill.color = Color.green;
        }
    }

    public void Reset()
    {
        _totalDuration = 0;
        timerSlider.value = 1f;
        timerText.text = "-";
    }
}
```

### Phase 6: MoveResult Handling

The `MoveResultPayload` has everything needed to update the game after each move:

```csharp
private void HandleMoveResult(MoveResultPayload payload)
{
    if (!payload.success && !string.IsNullOrEmpty(payload.error))
    {
        ShowError(payload.error);
        return;
    }

    var board = NetworkManager.Instance.Client.NullableBoardFromPayload(payload.board);
    _boardRenderer.UpdateBoard(board);

    if (payload.winner != null)
    {
        _boardRenderer.HighlightWinningCells(payload.winningCells);
        bool iWon = payload.winner.playerId == NetworkManager.Instance.Client.PlayerId;
        ShowGameOver(iWon ? "You Win!" : "You Lose");
        EnablePlayAgainButton(true);
    }
    else if (payload.isDraw)
    {
        ShowGameOver("Draw!");
        EnablePlayAgainButton(true);
    }
    else
    {
        bool myTurn = payload.currentPlayer?.playerId == NetworkManager.Instance.Client.PlayerId;
        _boardRenderer.SetInteractable(myTurn);
        UpdateTurnIndicator(payload.currentPlayer);
        EnablePlayAgainButton(false);
    }

    // Handle timeout win (payload.timeoutWin is true when opponent timed out)
    if (payload.timeoutWin)
    {
        bool iWon = payload.winner?.playerId == NetworkManager.Instance.Client.PlayerId;
        ShowGameOver(iWon ? "Opponent timed out — You win!" : "You timed out — You lose");
        EnablePlayAgainButton(true);
    }
}
```

### Phase 7: Reconnection (WebGL)

WebGL reconnection flow:

1. **Tab loses focus** → `OnApplicationFocus(false)` → no action needed (socket stays open briefly)
2. **Socket drops** (e.g. server restart, network blip) → `OnDisconnected` fires → show "Reconnecting..." overlay
3. **Socket auto-reconnects** (SocketManager has built-in retry: 5 attempts, 1s delay) → `OnConnected` fires
4. In `OnConnected`, if `_wasConnectedBefore && _client.IsInRoom` → call `Client.Reconnect()` to request `room-state`
5. Server responds with `OnRoomState` → full snapshot → restore board, timer, turn, etc.

```csharp
private void OnEnable()
{
    _client.OnConnected += HandleConnected;
    _client.OnDisconnected += HandleDisconnected;
    _client.OnRoomState += HandleRoomState;
}

private void HandleConnected()
{
    if (_wasConnectedBefore && _client.IsInRoom)
        _client.Reconnect();
    _wasConnectedBefore = true;
}

private void HandleRoomState(RoomStatePayload payload)
{
    HideReconnectingOverlay();
    var board = Client.NullableBoardFromPayload(payload.board);
    _boardRenderer.UpdateBoard(board);
    // Restore turn indicator, timer, etc.
}
```

### Phase 8: Room Lifecycle (WebGL)

| Action | Client Call | Server Response |
|---|---|---|
| Create Room | `Client.CreateRoom()` | `OnRoomCreated` |
| Join Room | `Client.JoinRoom(id)` | `OnRoomJoined` → `OnPlayerJoined` → `OnGameStarted` |
| Make Move | `Client.MakeMove(row, col)` | `OnMoveResult` |
| Play Again | `Client.PlayAgain()` | `OnGameStarted` |
| Leave Room | `Client.LeaveRoom()` | `OnPlayerLeft` → return to lobby |
| Reconnect | `Client.Reconnect()` (after `OnConnected`) | `OnRoomState` |
| Tab close | WebSocket drops | Server auto-handles via `disconnect` event |

### Phase 9: Error Handling (WebGL)

- **Server errors**: `Client.OnError` → show as UI toast/popup
- **Connection failures**: `Client.OnDisconnected` → show "Reconnecting..." (auto-retry)
- **Invalid moves**: `MoveResultPayload.error` → show brief message ("Not your turn", "Cell occupied")
- **CORS errors**: These appear in the browser console, not in Unity logs. If the server is on a different origin, verify `CLIENT_URL` is set correctly on the server.
- **WebSocket errors**: Logged to browser console. Enable "Development Build" in WebGL settings for detailed error messages.
- **Timeout**: If the turn timer reaches 0, the server will declare the opponent as winner and send `move-result` with `timeoutWin: true`.

### Phase 10: Building & Deployment

1. **Build** (`File → Build Settings → Build`): Output to a folder (e.g., `WebGLBuild/`).
2. **Serve locally** (development):
   ```bash
   npx serve WebGLBuild -p 5173
   ```
   Or use any static file server.
3. **Backend CORS**: Ensure `CLIENT_URL` matches the origin where the WebGL build is served.
4. **Production deployment**: Upload the WebGL build folder to a static host (Netlify, Vercel, GitHub Pages, etc.). Update `CLIENT_URL` on the server.

---

## SocketManager Internals (WebGL)

### Emit (Client → Server)

```csharp
// SocketManager passes the raw C# object — the library serializes it:
_socket.Emit(eventName, payload);  // payload is a C# object like CreateRoomPayload

// Do NOT pre-serialize to a string — that would double-quote it:
// _socket.Emit(eventName, JsonConvert.SerializeObject(payload)); // WRONG
```

### Receive (Server → Client)

The `SocketManager.RegisterHandler<T>()` uses `response.ToString()` to get the raw JSON, then `JsonHelper.Deserialize<T>()` to deserialize it. All events are dispatched to the Unity main thread via `UnityMainThreadDispatcher.Enqueue()`.

```csharp
private void RegisterHandler<T>(string eventName, Action<T> handler)
{
    _socket.On(eventName, response =>
    {
        UnityMainThreadDispatcher.Instance.Enqueue(() =>
        {
            try
            {
                string rawJson = response.ToString();
                T payload = JsonHelper.Deserialize<T>(rawJson);
                handler(payload);
            }
            catch (Exception ex) { /* Log error */ }
        });
    });
}
```

### Board Data

The backend sends the board as `List<List<string>>` — 3 rows × 3 columns.

```
null      → empty cell (PlayerSymbol? null in C#)
"X"       → PlayerSymbol.X
"O"       → PlayerSymbol.O
```

Use `TicTacToeSocketClient.NullableBoardFromPayload()` for conversion.

---

## Complete Event Reference

### Client → Server (6 events)

| Event | Payload | Trigger |
|---|---|---|
| `create-room` | `CreateRoomPayload { playerId }` | "Create Room" button |
| `join-room` | `JoinRoomPayload { roomId, playerId }` | "Join Room" button |
| `make-move` | `MakeMovePayload { roomId, playerId, row, col }` | Click a board cell |
| `reconnect` | `ReconnectPayload { playerId }` | After socket reconnects |
| `play-again` | `PlayAgainPayload { roomId, playerId }` | "Play Again" button |
| `leave-room` | `LeaveRoomPayload { roomId, playerId }` | "Exit" button |

### Server → Client (11 events)

| Event | Payload | Trigger |
|---|---|---|
| `room-created` | `RoomCreatedPayload { roomId, player }` | Room created |
| `room-joined` | `RoomJoinedPayload { roomId, player }` | Joined a room |
| `player-joined` | `PlayerJoinedPayload { playerId, symbol, players[] }` | Someone joined the room |
| `game-started` | `GameStartedPayload { board, currentPlayer, turnDeadline, playerX, playerO }` | Game begins or play-again |
| `move-result` | `MoveResultPayload { success, error?, move?, board?, currentPlayer?, turnDeadline?, winner?, winningCells?, isDraw?, timeoutWin? }` | After every move |
| `turn-timer` | `TurnTimerPayload { roomId, remainingMs, turnDeadline, currentPlayer }` | Every 1s while game is active |
| `player-disconnected` | `PlayerDisconnectedPayload { playerId }` | Player socket dropped |
| `player-reconnected` | `PlayerReconnectedPayload { playerId }` | Player socket restored |
| `player-left` | `PlayerLeftPayload { playerId, roomId, remainingPlayers, roomStatus }` | Player left the room |
| `room-state` | `RoomStatePayload { roomId, status, players[], board, currentPlayer, turnDeadline, winner, winningCells, isDraw, moves[] }` | Response to reconnect |
| `error` | `ErrorPayload { message }` | Server-side validation failure |

---

## WebGL Deployment Checklist

- [ ] SocketIOClient DLLs enabled for WebGL in Plugin Inspector
- [ ] `UnityMainThreadDispatcher` added to the scene
- [ ] Transport set to `WebSocket` only (no polling)
- [ ] Backend `CLIENT_URL` matches the WebGL host origin
- [ ] `PlayerPrefs.Save()` called after every write
- [ ] `OnApplicationFocus` handled for tab blur/restore
- [ ] Development build enabled during testing (for error details)
- [ ] CORS headers configured on production server
- [ ] Build compressed with gzip (not Brotli) for broad compatibility
