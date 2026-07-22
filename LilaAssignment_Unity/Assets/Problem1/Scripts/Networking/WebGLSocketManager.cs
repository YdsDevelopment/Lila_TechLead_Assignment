using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace TicTacToe
{
    public class WebGLSocketManager : MonoBehaviour, ISocketManager
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")] private static extern void TTT_SocketConnect(string serverUrl, string gameObjectName);
        [DllImport("__Internal")] private static extern void TTT_SocketEmit(string eventName, string payloadJson);
        [DllImport("__Internal")] private static extern void TTT_SocketDisconnect();
        [DllImport("__Internal")] private static extern int TTT_SocketIsConnected();
#endif

        public bool IsConnected
        {
            get
            {
#if UNITY_WEBGL && !UNITY_EDITOR
                Debug.LogError($"[WebGLSocketManager] TTT_SocketIsConnected: called");
                return TTT_SocketIsConnected() == 1;
#else
                return false;
#endif
            }
        }

        public event Action OnConnected;
        public event Action<string> OnDisconnected;
        public event Action<string> OnConnectError;
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
#if UNITY_WEBGL && !UNITY_EDITOR
Debug.LogError($"[WebGLSocketManager] Connect : called");
            TTT_SocketConnect(serverUrl, gameObject.name);
#endif
        }

        public void Disconnect()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
Debug.LogError($"[WebGLSocketManager] Disconnect : called");

            TTT_SocketDisconnect();
#endif
        }

        public void Emit(string eventName, object payload)
        {
            if (!IsConnected) return;
            var json = JsonHelper.Serialize(payload);
#if UNITY_WEBGL && !UNITY_EDITOR
Debug.LogError($"[WebGLSocketManager] TTT_SocketEmit : called");

            TTT_SocketEmit(eventName, json);
#endif
        }

        public void OnSocketBridgeMessage(string messageJson)
        {
            try
            {
                var envelope = JObject.Parse(messageJson);
                var eventName = envelope["eventName"]?.Value<string>();
                var payload = envelope["payload"];

                switch (eventName)
                {
                    case "socket-connected":
                        OnConnected?.Invoke();
                        break;
                    case "socket-disconnected":
                        var reason = payload?["reason"]?.Value<string>() ?? "unknown";
                        OnDisconnected?.Invoke(reason);
                        break;
                    case "socket-error":
                        var msg = payload?["message"]?.Value<string>() ?? "Socket error";
                        OnConnectError?.Invoke(msg);
                        break;
                    case "socket-reconnect-attempt":
                        break;
                    default:
                        RouteServerEvent(eventName, payload);
                        break;
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[WebGLSocketManager] Failed to parse bridge message: {ex.Message}");
            }
        }

        private void RouteServerEvent(string eventName, JToken payload)
        {
            if (payload == null) return;

            switch (eventName)
            {
                case "room-created":
                    OnRoomCreated?.Invoke(payload.ToObject<RoomCreatedPayload>());
                    break;
                case "room-joined":
                    OnRoomJoined?.Invoke(payload.ToObject<RoomJoinedPayload>());
                    break;
                case "player-joined":
                    OnPlayerJoined?.Invoke(payload.ToObject<PlayerJoinedPayload>());
                    break;
                case "game-started":
                    OnGameStarted?.Invoke(payload.ToObject<GameStartedPayload>());
                    break;
                case "move-result":
                    OnMoveResult?.Invoke(payload.ToObject<MoveResultPayload>());
                    break;
                case "turn-timer":
                    OnTurnTimer?.Invoke(payload.ToObject<TurnTimerPayload>());
                    break;
                case "player-disconnected":
                    OnPlayerDisconnected?.Invoke(payload.ToObject<PlayerDisconnectedPayload>());
                    break;
                case "player-reconnected":
                    OnPlayerReconnected?.Invoke(payload.ToObject<PlayerReconnectedPayload>());
                    break;
                case "player-left":
                    OnPlayerLeft?.Invoke(payload.ToObject<PlayerLeftPayload>());
                    break;
                case "room-state":
                    OnRoomState?.Invoke(payload.ToObject<RoomStatePayload>());
                    break;
                case "rooms-list":
                    OnRoomsList?.Invoke(payload.ToObject<RoomsListPayload>());
                    break;
                case "room-details":
                    OnRoomDetails?.Invoke(payload.ToObject<RoomDetailsPayload>());
                    break;
                case "health-status":
                    OnHealthStatus?.Invoke(payload.ToObject<HealthStatusPayload>());
                    break;
                case "error":
                    OnError?.Invoke(payload.ToObject<ErrorPayload>());
                    break;
            }
        }
    }
}
