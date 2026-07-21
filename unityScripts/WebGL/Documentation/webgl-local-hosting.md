# Unity WebGL: Socket.IO and local hosting

## What to use

Use `TicTacToeWebGLSocketClient.cs` for a **WebGL player** and use `TicTacToeSocketClient.cs` for Editor, desktop, Android, or iOS. Do not put both components on the same GameObject.

The WebGL client has this transport path:

```text
Unity C# → TicTacToeSocketBridge.jslib → browser Socket.IO v4 client → backend
backend → browser Socket.IO v4 client → .jslib SendMessage → Unity C#
```

The bridge uses the same `TicTacToeContracts.cs` payload types and backend event names as the non-WebGL client.

## Add the files to Unity

Copy these repository files to the Unity project:

```text
unityScripts/Runtime/TicTacToeContracts.cs
unityScripts/WebGL/Runtime/TicTacToeWebGLSocketClient.cs
unityScripts/WebGL/Plugins/WebGL/TicTacToeSocketBridge.jslib
```

Install Unity's Newtonsoft JSON package (`com.unity.nuget.newtonsoft-json`) with Package Manager. It is used to safely serialize outgoing C# request objects and deserialize arbitrary Socket.IO JSON.

Create a `NetworkManager` GameObject in the boot scene and add `TicTacToeWebGLSocketClient`. It persists through scene changes and connects automatically. Subscribe to its public events from your UI presenter exactly as you would with the standard socket client.

## Load Socket.IO in the WebGL page

The `.jslib` bridge needs the browser `io` function. Create a custom WebGL template or edit the development generated `index.html`, and add this **before** the Unity loader script:

```html
<script src="http://localhost:3000/socket.io/socket.io.js"></script>
```

The Node Socket.IO server in this repository serves this client script by default. For production, serve a versioned Socket.IO browser client from your own HTTPS origin rather than relying on a development URL.

## Run locally

### Terminal 1: backend

The browser origin must match `CLIENT_URL` because this backend's Express and Socket.IO CORS configuration accept one configured origin.

```bash
cd backend
cp .env.example .env
```

Set this in `backend/.env`:

```env
PORT=3000
CLIENT_URL=http://localhost:8080
TURN_TIMEOUT=30000
TURN_TIMEOUT_ENABLED=true
```

Then start it:

```bash
npm install
npm run dev
```

### Terminal 2: WebGL game

In Unity:

1. Open **File → Build Settings**.
2. Select **WebGL** and choose **Switch Platform**.
3. For a simple Python static server, set **Player Settings → Publishing Settings → Compression Format** to **Disabled**.
4. Build to a folder named `WebGLBuild`.

Serve the folder rather than opening `index.html` using `file://`:

```bash
cd WebGLBuild
python3 -m http.server 8080
```

Open `http://localhost:8080` in two browser tabs. Create a room in tab one, then join it from tab two. Each tab needs a different `playerId`; use a separate browser profile/incognito window or clear the `tic-tac-toe.player-id` PlayerPrefs/local storage key before opening the second player.

Alternatively, Unity **Build And Run** hosts the WebGL build on a local URL. In that case, inspect its port and set `CLIENT_URL` to that exact origin before starting the backend.

## Browser/UI flow

1. `Connected` means browser Socket.IO has connected.
2. Call `CreateRoom()` or `JoinRoom(roomId)`.
3. Store/display `RoomId` and `Symbol` after `RoomCreated`/`RoomJoined`.
4. Render state only after `GameStarted`, `MoveResult`, or `RoomStateReceived`.
5. A board button should call `MakeMove(row, col)` only if it is the local player's turn and its server board cell is empty.
6. Calculate a displayed countdown from `turnDeadline - DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()`.
7. On browser-level reconnect (`Connected` after a prior disconnect), call `RecoverRoom()`. Replace all local state from `RoomStateReceived`.

## Troubleshooting

| Symptom | Cause / fix |
|---|---|
| `Socket.IO browser client is not loaded` | Add the Socket.IO `<script>` tag before the Unity loader in the WebGL template. |
| Browser CORS error | The game origin differs from `CLIENT_URL`. Set it to exactly the static server origin and restart backend. |
| WebGL page cannot load | Do not use `file://`; use Build And Run or a local static server. |
| Two tabs get “Player already in a room” | They share PlayerPrefs/local storage. Use different browser profiles/incognito. |
| Room cannot be recovered | Backend state is in memory; a Node restart removes rooms. |
| HTTP build works locally but not production | Host game and API over HTTPS; browser security blocks mixed HTTPS/HTTP content. |

## Production note

This backend currently has no authentication and treats the client-provided `playerId` as identity. The WebGL bridge protects neither identity nor game authorization; add an authenticated session/token and server-side socket ownership checks before exposing it publicly.
