using System;
using System.Runtime.InteropServices;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace TicTacToeMultiplayer
{
    /// <summary>
    /// WebGL implementation of the Tic-Tac-Toe Socket.IO client.
    /// It calls the browser Socket.IO client through TicTacToeSocketBridge.jslib.
    /// Requires TicTacToeContracts.cs and Newtonsoft JSON.
    /// </summary>
    public sealed class TicTacToeWebGLSocketClient : MonoBehaviour
    {
        [SerializeField] private string serverUrl = "http://localhost:3000";
        [SerializeField] private string playerId;

        private const string PlayerIdPreferenceKey = "tic-tac-toe.player-id";

        public string PlayerId => playerId;
        public string RoomId { get; private set; }
        public string Symbol { get; private set; }
        public bool IsConnected => IsSocketConnected();

        public event Action Connected;
        public event Action<string> Disconnected;
        public event Action<RoomCreatedPayload> RoomCreated;
        public event Action<RoomJoinedPayload> RoomJoined;
        public event Action<PlayerJoinedPayload> PlayerJoined;
        public event Action<GameStartedPayload> GameStarted;
        public event Action<MoveResultPayload> MoveResult;
        public event Action<PlayerConnectionPayload> PlayerDisconnected;
        public event Action<PlayerConnectionPayload> PlayerReconnected;
        public event Action<RoomStatePayload> RoomStateReceived;
        public event Action<ErrorPayload> ServerError;

#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")] private static extern void TTT_SocketConnect(string serverUrl, string gameObjectName);
        [DllImport("__Internal")] private static extern void TTT_SocketEmit(string eventName, string payloadJson);
        [DllImport("__Internal")] private static extern void TTT_SocketDisconnect();
        [DllImport("__Internal")] private static extern int TTT_SocketIsConnected();
#endif

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
            EnsurePlayerId();
        }

        private void Start()
        {
            Connect();
        }

        private void EnsurePlayerId()
        {
            if (string.IsNullOrWhiteSpace(playerId))
            {
                playerId = PlayerPrefs.GetString(PlayerIdPreferenceKey, Guid.NewGuid().ToString());
            }

            PlayerPrefs.SetString(PlayerIdPreferenceKey, playerId);
            PlayerPrefs.Save();
        }

        public void Connect()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            TTT_SocketConnect(serverUrl, gameObject.name);
#else
            Debug.LogWarning("TicTacToeWebGLSocketClient runs only in a WebGL player. Use TicTacToeSocketClient for Editor, desktop, and mobile.");
#endif
        }

        public void Disconnect()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            TTT_SocketDisconnect();
#endif
        }

        public void CreateRoom()
        {
            Emit(SocketEvents.CreateRoom, new CreateRoomRequest { PlayerId = playerId });
        }

        public void JoinRoom(string roomId)
        {
            if (string.IsNullOrWhiteSpace(roomId))
            {
                throw new ArgumentException("A room ID is required.", nameof(roomId));
            }

            Emit(SocketEvents.JoinRoom, new JoinRoomRequest { RoomId = roomId, PlayerId = playerId });
        }

        public void MakeMove(int row, int col)
        {
            if (string.IsNullOrWhiteSpace(RoomId))
            {
                throw new InvalidOperationException("Create or join a room before making a move.");
            }

            Emit(SocketEvents.MakeMove, new MakeMoveRequest
            {
                RoomId = RoomId,
                PlayerId = playerId,
                Row = row,
                Col = col
            });
        }

        /// <summary>Call after a browser-level reconnect to receive a full room-state snapshot.</summary>
        public void RecoverRoom()
        {
            Emit(SocketEvents.Reconnect, new ReconnectRequest { PlayerId = playerId });
        }

        // Called by the .jslib bridge through Unity SendMessage. Do not rename.
        public void OnSocketBridgeMessage(string messageJson)
        {
            SocketBridgeEnvelope envelope;
            try
            {
                envelope = JsonConvert.DeserializeObject<SocketBridgeEnvelope>(messageJson);
            }
            catch (JsonException exception)
            {
                ServerError?.Invoke(new ErrorPayload { Message = "Invalid Socket.IO bridge message: " + exception.Message });
                return;
            }

            if (envelope == null || string.IsNullOrWhiteSpace(envelope.EventName))
            {
                ServerError?.Invoke(new ErrorPayload { Message = "Socket.IO bridge returned an empty event." });
                return;
            }

            switch (envelope.EventName)
            {
                case "socket-connected":
                    Connected?.Invoke();
                    break;
                case "socket-disconnected":
                    Disconnected?.Invoke(envelope.Payload?["reason"]?.Value<string>() ?? "Disconnected");
                    break;
                case "socket-error":
                    ServerError?.Invoke(Read<ErrorPayload>(envelope.Payload));
                    break;
                case SocketEvents.RoomCreated:
                    var created = Read<RoomCreatedPayload>(envelope.Payload);
                    RoomId = created.RoomId;
                    Symbol = created.Player?.Symbol;
                    RoomCreated?.Invoke(created);
                    break;
                case SocketEvents.RoomJoined:
                    var joined = Read<RoomJoinedPayload>(envelope.Payload);
                    RoomId = joined.RoomId;
                    Symbol = joined.Player?.Symbol;
                    RoomJoined?.Invoke(joined);
                    break;
                case SocketEvents.PlayerJoined:
                    PlayerJoined?.Invoke(Read<PlayerJoinedPayload>(envelope.Payload));
                    break;
                case SocketEvents.GameStarted:
                    GameStarted?.Invoke(Read<GameStartedPayload>(envelope.Payload));
                    break;
                case SocketEvents.MoveResult:
                    MoveResult?.Invoke(Read<MoveResultPayload>(envelope.Payload));
                    break;
                case SocketEvents.PlayerDisconnected:
                    PlayerDisconnected?.Invoke(Read<PlayerConnectionPayload>(envelope.Payload));
                    break;
                case SocketEvents.PlayerReconnected:
                    PlayerReconnected?.Invoke(Read<PlayerConnectionPayload>(envelope.Payload));
                    break;
                case SocketEvents.RoomState:
                    var state = Read<RoomStatePayload>(envelope.Payload);
                    RoomId = state.RoomId;
                    var me = state.Players == null ? null : Array.Find(state.Players, player => player.PlayerId == playerId);
                    Symbol = me == null ? Symbol : me.Symbol;
                    RoomStateReceived?.Invoke(state);
                    break;
                case SocketEvents.Error:
                    ServerError?.Invoke(Read<ErrorPayload>(envelope.Payload));
                    break;
                default:
                    ServerError?.Invoke(new ErrorPayload { Message = "Unknown Socket.IO event: " + envelope.EventName });
                    break;
            }
        }

        private void Emit(string eventName, object payload)
        {
            if (!IsConnected)
            {
                ServerError?.Invoke(new ErrorPayload { Message = "Socket.IO is not connected." });
                return;
            }

#if UNITY_WEBGL && !UNITY_EDITOR
            TTT_SocketEmit(eventName, JsonConvert.SerializeObject(payload));
#endif
        }

        private static T Read<T>(JToken token) where T : new()
        {
            return token == null ? new T() : token.ToObject<T>();
        }

        private static bool IsSocketConnected()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            return TTT_SocketIsConnected() == 1;
#else
            return false;
#endif
        }

        private void OnDestroy()
        {
            Disconnect();
        }

        [Serializable]
        private sealed class SocketBridgeEnvelope
        {
            [JsonProperty("eventName")]
            public string EventName;

            [JsonProperty("payload")]
            public JToken Payload;
        }
    }
}
