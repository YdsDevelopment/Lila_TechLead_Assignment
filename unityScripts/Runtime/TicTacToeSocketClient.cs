using System;
using System.Threading.Tasks;
using SocketIOClient;
using SocketIOClient.JsonSerializer;
using SocketIOClient.Transport;
using UnityEngine;

namespace TicTacToeMultiplayer
{
    /// <summary>
    /// A persistent, typed Unity facade for this repository's Socket.IO protocol.
    /// Subscribe to the events from UI/gameplay code; do not call SocketIOUnity directly.
    /// </summary>
    public sealed class TicTacToeSocketClient : MonoBehaviour
    {
        [Header("Backend")]
        [Tooltip("Use a LAN IP on a physical device. localhost only works when Unity and backend run on the same machine.")]
        [SerializeField] private string serverUrl = "http://localhost:3000";

        [Header("Identity")]
        [SerializeField] private string playerId;

        private const string PlayerIdPreferenceKey = "tic-tac-toe.player-id";
        private SocketIOUnity socket;

        public string PlayerId => playerId;
        public string RoomId { get; private set; }
        public string Symbol { get; private set; }
        public bool IsConnected => socket != null && socket.Connected;

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

        private async void Awake()
        {
            DontDestroyOnLoad(gameObject);
            EnsurePlayerId();
            ConfigureSocket();
            await ConnectAsync();
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

        private void ConfigureSocket()
        {
            socket = new SocketIOUnity(
                new Uri(serverUrl),
                new SocketIOOptions { Transport = TransportProtocol.WebSocket });

            // SocketIOUnity recommends this serializer for Unity IL2CPP builds.
            socket.JsonSerializer = new NewtonsoftJsonSerializer();
            socket.unityThreadScope = SocketIOUnity.UnityThreadScope.Update;

            socket.OnConnected += (_, __) => Connected?.Invoke();
            socket.OnDisconnected += (_, reason) => Disconnected?.Invoke(reason);

            socket.OnUnityThread(SocketEvents.RoomCreated, response =>
            {
                var payload = response.GetValue<RoomCreatedPayload>();
                RoomId = payload.RoomId;
                Symbol = payload.Player.Symbol;
                RoomCreated?.Invoke(payload);
            });

            socket.OnUnityThread(SocketEvents.RoomJoined, response =>
            {
                var payload = response.GetValue<RoomJoinedPayload>();
                RoomId = payload.RoomId;
                Symbol = payload.Player.Symbol;
                RoomJoined?.Invoke(payload);
            });

            socket.OnUnityThread(SocketEvents.PlayerJoined,
                response => PlayerJoined?.Invoke(response.GetValue<PlayerJoinedPayload>()));
            socket.OnUnityThread(SocketEvents.GameStarted,
                response => GameStarted?.Invoke(response.GetValue<GameStartedPayload>()));
            socket.OnUnityThread(SocketEvents.MoveResult,
                response => MoveResult?.Invoke(response.GetValue<MoveResultPayload>()));
            socket.OnUnityThread(SocketEvents.PlayerDisconnected,
                response => PlayerDisconnected?.Invoke(response.GetValue<PlayerConnectionPayload>()));
            socket.OnUnityThread(SocketEvents.PlayerReconnected,
                response => PlayerReconnected?.Invoke(response.GetValue<PlayerConnectionPayload>()));

            socket.OnUnityThread(SocketEvents.RoomState, response =>
            {
                var payload = response.GetValue<RoomStatePayload>();
                RoomId = payload.RoomId;
                var me = Array.Find(payload.Players, player => player.PlayerId == playerId);
                Symbol = me == null ? Symbol : me.Symbol;
                RoomStateReceived?.Invoke(payload);
            });

            socket.OnUnityThread(SocketEvents.Error,
                response => ServerError?.Invoke(response.GetValue<ErrorPayload>()));
        }

        public async Task ConnectAsync()
        {
            if (IsConnected)
            {
                return;
            }

            await socket.ConnectAsync();
        }

        public void Disconnect()
        {
            socket?.Disconnect();
        }

        public void CreateRoom()
        {
            RequireConnection();
            socket.Emit(SocketEvents.CreateRoom, new CreateRoomRequest { PlayerId = playerId });
        }

        public void JoinRoom(string roomId)
        {
            RequireConnection();
            if (string.IsNullOrWhiteSpace(roomId))
            {
                throw new ArgumentException("A room ID is required.", nameof(roomId));
            }

            socket.Emit(SocketEvents.JoinRoom, new JoinRoomRequest { RoomId = roomId, PlayerId = playerId });
        }

        public void MakeMove(int row, int col)
        {
            RequireConnection();
            if (string.IsNullOrWhiteSpace(RoomId))
            {
                throw new InvalidOperationException("Create or join a room before making a move.");
            }

            socket.Emit(SocketEvents.MakeMove, new MakeMoveRequest
            {
                RoomId = RoomId,
                PlayerId = playerId,
                Row = row,
                Col = col
            });
        }

        /// <summary>Call after a native Socket.IO reconnection to recover the server snapshot.</summary>
        public void RecoverRoom()
        {
            RequireConnection();
            socket.Emit(SocketEvents.Reconnect, new ReconnectRequest { PlayerId = playerId });
        }

        private void RequireConnection()
        {
            if (!IsConnected)
            {
                throw new InvalidOperationException("Socket.IO is not connected.");
            }
        }

        private void OnDestroy()
        {
            socket?.Disconnect();
        }
    }
}
