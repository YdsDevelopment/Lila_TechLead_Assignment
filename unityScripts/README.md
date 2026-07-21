# Unity integration scripts

These scripts provide a typed Unity integration for the current Tic-Tac-Toe backend. They intentionally mirror the backend's runtime contract, including `room-joined`, nullable end-game fields, and optional `timeoutWin`.

## Files

| File | Use |
|---|---|
| `Runtime/TicTacToeContracts.cs` | Every implemented Socket.IO request/response DTO plus REST DTOs and event names. |
| `Runtime/TicTacToeSocketClient.cs` | Persistent Socket.IO manager with typed C# events and command methods. |
| `Runtime/TicTacToeRestClient.cs` | Typed `GET /health`, `GET /rooms`, and `GET /rooms/:id` client. |
| `Documentation/unity-workflow.md` | Installation, scene wiring, UI flow, reconnect behavior, and device networking. |
| `WebGL/` | Browser Socket.IO `.jslib` bridge, typed WebGL socket client, and local-hosting workflow. |

Follow [the Unity workflow](./Documentation/unity-workflow.md) before copying the scripts into `Assets/Scripts`.
For a browser build, follow the [WebGL workflow](./WebGL/Documentation/webgl-local-hosting.md) instead of using the standalone socket client.
