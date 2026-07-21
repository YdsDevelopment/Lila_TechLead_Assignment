using System;
using Newtonsoft.Json.Linq;
using SocketIOClient;
using UnityEngine;

namespace TicTacToe
{
    public class SocketManager : ISocketManager
    {
        private SocketIOUnity _socket;

        public bool IsConnected => _socket != null && _socket.Connected;

        // --- Connection events ---
        public event Action OnConnected;
        public event Action<string> OnDisconnected;
        public event Action<string> OnConnectError;

        // --- Server event handlers ---
        public event Action<RoomCreatedPayload> OnRoomCreated;
        public event Action<RoomJoinedPayload> OnRoomJoined;
        public event Action<PlayerJoinedPayload> OnPlayerJoined;
        public event Action<GameStartedPayload> OnGameStarted;
        public event Action<MoveResultPayload> OnMoveResult;
        public event Action<TurnTimerPayload> OnTurnTimer;
        public event Action<PlayerDisconnectedPayload> OnPlayerDisconnected;
        public event Action<PlayerReconnectedPayload> OnPlayerReconnected;
        public event Action<PlayerLeftPayload> OnPlayerLeft;
        public event Action<RoomStatePayload> OnRoomState;
        public event Action<ErrorPayload> OnError;

        public void Connect(string serverUrl)
        {
            if (_socket != null)
            {
                Disconnect();
            }

            var uri = new Uri(serverUrl);
            _socket = new SocketIOUnity(uri, new SocketIOOptions
            {
                Transport = SocketIOClient.Transport.TransportProtocol.WebSocket,
                Reconnection = true,
                ReconnectionAttempts = 5,
                ReconnectionDelay = 1000
            });

            RegisterCoreHandlers();
            RegisterEventHandlers();

            _socket.Connect();
        }

        public void Disconnect()
        {
            if (_socket != null)
            {
                _socket.Disconnect();
                _socket.Dispose();
                _socket = null;
            }
        }

        public void Emit(string eventName, object payload)
        {
            if (_socket == null || !_socket.Connected)
            {
                Debug.LogWarning($"SocketManager: Cannot emit '{eventName}' — not connected");
                return;
            }

            _socket.Emit(eventName, payload);
        }

        // ---------------------------------------------------------------
        private void RegisterCoreHandlers()
        {
            _socket.OnConnected += (sender, e) =>
            {
                Debug.Log("[SocketManager] Connected");
                OnConnected?.Invoke();
            };

            _socket.OnDisconnected += (sender, reason) =>
            {
                Debug.Log($"[SocketManager] Disconnected: {reason}");
                OnDisconnected?.Invoke(reason);
            };

            _socket.OnError += (sender, error) =>
            {
                Debug.LogError($"[SocketManager] Error: {error}");
                OnConnectError?.Invoke(error);
            };

            _socket.OnReconnectAttempt += (sender, attempt) =>
            {
                Debug.Log($"[SocketManager] Reconnect attempt {attempt}");
            };
        }

        private void RegisterEventHandlers()
        {
            RegisterHandler<RoomCreatedPayload>(SocketEventNames.ROOM_CREATED, payload => OnRoomCreated?.Invoke(payload));
            RegisterHandler<RoomJoinedPayload>(SocketEventNames.ROOM_JOINED, payload => OnRoomJoined?.Invoke(payload));
            RegisterHandler<PlayerJoinedPayload>(SocketEventNames.PLAYER_JOINED, payload => OnPlayerJoined?.Invoke(payload));
            RegisterHandler<GameStartedPayload>(SocketEventNames.GAME_STARTED, payload => OnGameStarted?.Invoke(payload));
            RegisterHandler<MoveResultPayload>(SocketEventNames.MOVE_RESULT, payload => OnMoveResult?.Invoke(payload));
            RegisterHandler<TurnTimerPayload>(SocketEventNames.TURN_TIMER, payload => OnTurnTimer?.Invoke(payload));
            RegisterHandler<PlayerDisconnectedPayload>(SocketEventNames.PLAYER_DISCONNECTED, payload => OnPlayerDisconnected?.Invoke(payload));
            RegisterHandler<PlayerReconnectedPayload>(SocketEventNames.PLAYER_RECONNECTED, payload => OnPlayerReconnected?.Invoke(payload));
            RegisterHandler<PlayerLeftPayload>(SocketEventNames.PLAYER_LEFT, payload => OnPlayerLeft?.Invoke(payload));
            RegisterHandler<RoomStatePayload>(SocketEventNames.ROOM_STATE, payload => OnRoomState?.Invoke(payload));
            RegisterHandler<ErrorPayload>(SocketEventNames.ERROR, payload => OnError?.Invoke(payload));
        }

        private void RegisterHandler<T>(string eventName, Action<T> handler)
        {
            _socket.On(eventName, response =>
            {
                try
                {
                    string rawJson = response.ToString();
                    T payload = JsonHelper.Deserialize<T>(rawJson);
                    handler(payload);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[SocketManager] Failed to deserialize '{eventName}': {ex.Message}\n{response}");
                }
            });
        }
    }
}
