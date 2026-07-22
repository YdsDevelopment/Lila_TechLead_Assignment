using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using SocketIOClient;
using TicTacToe.Utils;
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
        public event Action<HealthStatusPayload> OnHealthStatus;
        public event Action<RoomsListPayload> OnRoomsList;
        public event Action<RoomDetailsPayload> OnRoomDetails;

        public void Connect(string serverUrl)
        {
            Debug.LogError("SocketManager Connect is Called : 1 " + serverUrl);
            if (_socket != null)
            {
                Disconnect();
            }
            Debug.LogError("SocketManager Connect is Called : 2 " + serverUrl);

            var uri = new Uri(serverUrl);
            _socket = new SocketIOUnity(uri, new SocketIOOptions
            {
                Transport = SocketIOClient.Transport.TransportProtocol.WebSocket,
                Reconnection = true,
                ReconnectionAttempts = 5,
                ReconnectionDelay = 1000
            });
            Debug.LogError("SocketManager Connect is Called 3 : " + serverUrl);
            RegisterCoreHandlers();
            RegisterEventHandlers();
            Debug.LogError("SocketManager Connect is Called : 4 " + serverUrl);
            _socket.Connect();
            Debug.LogError("SocketManager Connect is Called : 5 " + serverUrl);
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
                UnityMainThreadDispatcher.Instance.Enqueue(() =>
                {
                    Debug.Log("[SocketManager] Connected");
                    OnConnected?.Invoke();
                });
            };

            _socket.OnDisconnected += (sender, reason) =>
            {
                UnityMainThreadDispatcher.Instance.Enqueue(() =>
                {
                    Debug.Log($"[SocketManager] Disconnected: {reason}");
                    OnDisconnected?.Invoke(reason);
                });
            };

            _socket.OnError += (sender, error) =>
            {
                UnityMainThreadDispatcher.Instance.Enqueue(() =>
                {
                    Debug.LogError($"[SocketManager] Error: {error}");
                    OnConnectError?.Invoke(error);
                });
            };

            _socket.OnReconnectAttempt += (sender, attempt) =>
            {
                UnityMainThreadDispatcher.Instance.Enqueue(() =>
                {
                    Debug.LogError($"[SocketManager] Reconnect attempt {attempt}");
                });
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
            RegisterHandler<HealthStatusPayload>(SocketEventNames.HEALTH_STATUS, payload => OnHealthStatus?.Invoke(payload));
            RegisterHandler<RoomsListPayload>(SocketEventNames.ROOMS_LIST, payload => OnRoomsList?.Invoke(payload));
            RegisterHandler<RoomDetailsPayload>(SocketEventNames.ROOM_DETAILS, payload => OnRoomDetails?.Invoke(payload));
        }

        private void RegisterHandler<T>(string eventName, Action<T> handler)
        {
            _socket.On(eventName, response =>
            {
                UnityMainThreadDispatcher.Instance.Enqueue(() =>
                {
                    try
                    {
                        string rawJson = response.ToString().Trim();
                        T payload;
                        if (rawJson.StartsWith("["))
                        {
                            var list = JsonHelper.Deserialize<List<T>>(rawJson);
                            payload = list != null && list.Count > 0 ? list[0] : default;
                        }
                        else
                        {
                            payload = JsonHelper.Deserialize<T>(rawJson);
                        }
                        handler(payload);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"[SocketManager] Failed to deserialize '{eventName}': {ex.Message}\n{response}");
                    }
                });
            });
        }
    }
}
