using System;
using System.Collections.Generic;
using UnityEngine;

namespace TicTacToe
{
    public class TicTacToeSocketClient
    {
        private readonly ISocketManager _socket;
        private string _playerId;
        private string _roomId;
        private PlayerSymbol _mySymbol;

        public string PlayerId => _playerId;
        public string RoomId => _roomId;
        public PlayerSymbol MySymbol => _mySymbol;
        public bool IsConnected => _socket.IsConnected;
        public bool IsInRoom => !string.IsNullOrEmpty(_roomId);

        // --- Events for the Unity game to listen to ---
        public event Action OnConnected;
        public event Action<string> OnDisconnected;
        public event Action<string> OnError;

        public event Action<string, PlayerSummary> OnRoomCreated;
        public event Action<string, PlayerSummary> OnRoomJoined;
        public event Action OnGameStarted;
        public event Action<MoveResultPayload> OnMoveResult;
        public event Action<long, long> OnTurnTimer; // remainingMs, turnDeadline
        public event Action<string> OnPlayerDisconnected;
        public event Action<string> OnPlayerReconnected;
        public event Action<PlayerLeftPayload> OnPlayerLeft;
        public event Action<RoomStatePayload> OnRoomState;
        public event Action<PlayerJoinedPayload> OnPlayerJoined;

        public TicTacToeSocketClient(ISocketManager socketManager)
        {
            _socket = socketManager;
            RegisterHandlers();
        }

        // ---------------------------------------------------------------
        // Connection
        // ---------------------------------------------------------------
        public void Connect(string serverUrl, string playerId)
        {
            _playerId = playerId;
            _socket.Connect(serverUrl);
        }

        public void Disconnect()
        {
            _socket.Disconnect();
            _roomId = null;
        }

        // ---------------------------------------------------------------
        // Room Actions
        // ---------------------------------------------------------------
        public void CreateRoom()
        {
            _socket.Emit(SocketEventNames.CREATE_ROOM, new CreateRoomPayload
            {
                playerId = _playerId
            });
        }

        public void JoinRoom(string roomId)
        {
            _socket.Emit(SocketEventNames.JOIN_ROOM, new JoinRoomPayload
            {
                roomId = roomId,
                playerId = _playerId
            });
        }

        public void MakeMove(int row, int col)
        {
            _socket.Emit(SocketEventNames.MAKE_MOVE, new MakeMovePayload
            {
                roomId = _roomId,
                playerId = _playerId,
                row = row,
                col = col
            });
        }

        public void PlayAgain()
        {
            _socket.Emit(SocketEventNames.PLAY_AGAIN, new PlayAgainPayload
            {
                roomId = _roomId,
                playerId = _playerId
            });
        }

        public void LeaveRoom()
        {
            _socket.Emit(SocketEventNames.LEAVE_ROOM, new LeaveRoomPayload
            {
                roomId = _roomId,
                playerId = _playerId
            });
            _roomId = null;
        }

        public void Reconnect()
        {
            _socket.Emit(SocketEventNames.RECONNECT, new ReconnectPayload
            {
                playerId = _playerId
            });
        }

        // ---------------------------------------------------------------
        // Board Helpers
        // ---------------------------------------------------------------
        public PlayerSymbol?[,] NullableBoardFromPayload(List<List<string>> boardData)
        {
            var board = new PlayerSymbol?[3, 3];
            if (boardData == null || boardData.Count != 3) return board;

            for (int r = 0; r < 3; r++)
            {
                for (int c = 0; c < 3; c++)
                {
                    string val = boardData[r]?[c];
                    board[r, c] = string.IsNullOrEmpty(val)
                        ? (PlayerSymbol?)null
                        : (val == "X" ? PlayerSymbol.X : PlayerSymbol.O);
                }
            }
            return board;
        }

        // ---------------------------------------------------------------
        // Internal
        // ---------------------------------------------------------------
        private void RegisterHandlers()
        {
            _socket.OnConnected += () =>
            {
                Debug.Log("[TicTacToeClient] Connected to server");
                OnConnected?.Invoke();
            };

            _socket.OnDisconnected += (reason) =>
            {
                Debug.Log($"[TicTacToeClient] Disconnected: {reason}");
                OnDisconnected?.Invoke(reason);
            };

            _socket.OnError += (message) =>
            {
                Debug.LogError($"[TicTacToeClient] Error: {message}");
                OnError?.Invoke(message.message);
            };

            _socket.OnRoomCreated += (payload) =>
            {
                _roomId = payload.roomId;
                _mySymbol = payload.player.symbol;
                Debug.Log($"[TicTacToeClient] Room created: {payload.roomId} as {payload.player.symbol}");
                OnRoomCreated?.Invoke(payload.roomId, payload.player);
            };

            _socket.OnRoomJoined += (payload) =>
            {
                _roomId = payload.roomId;
                _mySymbol = payload.player.symbol;
                Debug.Log($"[TicTacToeClient] Room joined: {payload.roomId} as {payload.player.symbol}");
                OnRoomJoined?.Invoke(payload.roomId, payload.player);
            };

            _socket.OnPlayerJoined += (payload) =>
            {
                Debug.Log($"[TicTacToeClient] Player joined: {payload.playerId}");
                OnPlayerJoined?.Invoke(payload);
            };

            _socket.OnGameStarted += (payload) =>
            {
                Debug.Log("[TicTacToeClient] Game started");
                OnGameStarted?.Invoke();
            };

            _socket.OnMoveResult += (payload) =>
            {
                OnMoveResult?.Invoke(payload);
            };

            _socket.OnTurnTimer += (payload) =>
            {
                OnTurnTimer?.Invoke(payload.remainingMs, payload.turnDeadline);
            };

            _socket.OnPlayerDisconnected += (payload) =>
            {
                Debug.Log($"[TicTacToeClient] Player disconnected: {payload.playerId}");
                OnPlayerDisconnected?.Invoke(payload.playerId);
            };

            _socket.OnPlayerReconnected += (payload) =>
            {
                Debug.Log($"[TicTacToeClient] Player reconnected: {payload.playerId}");
                OnPlayerReconnected?.Invoke(payload.playerId);
            };

            _socket.OnPlayerLeft += (payload) =>
            {
                Debug.Log($"[TicTacToeClient] Player left: {payload.playerId}");
                OnPlayerLeft?.Invoke(payload);
                if (payload.playerId == _playerId)
                {
                    _roomId = null;
                }
            };

            _socket.OnRoomState += (payload) =>
            {
                _roomId = payload.roomId;
                foreach (var p in payload.players)
                {
                    if (p.playerId == _playerId)
                    {
                        _mySymbol = p.symbol;
                        break;
                    }
                }
                OnRoomState?.Invoke(payload);
            };
        }
    }
}
