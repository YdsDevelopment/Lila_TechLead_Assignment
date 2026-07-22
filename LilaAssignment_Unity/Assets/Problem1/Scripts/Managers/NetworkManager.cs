using TicTacToe.Utils;
using UnityEngine;

namespace TicTacToe
{
    public class NetworkManager : MonoBehaviour
    {
        public static NetworkManager Instance { get; private set; }

        private TicTacToeSocketClient _client;
        public TicTacToeSocketClient Client => _client;

        [SerializeField] private string _serverUrl = "http://localhost:3000";
        public string ServerUrl => _serverUrl;

        public void SetServerUrl(string url)
        {
            if (!string.IsNullOrEmpty(url))
                _serverUrl = url;
        }
        private string _playerId;
        private bool _wasConnectedBefore;
        private bool _reconnectOverlayShowing;

        private void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            _ = UnityMainThreadDispatcher.Instance;

            _playerId = GetOrCreatePlayerId();
            var socketManager = new SocketManager();
            _client = new TicTacToeSocketClient(socketManager);
            RegisterClientEvents();
        }

        private void Start()
        {
            // Connect();
        }

        public string GetOrCreatePlayerId()
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

        public string RegeratePlayerID()
        {
            var id = System.Guid.NewGuid().ToString();
            PlayerPrefs.SetString("playerId", id);
            PlayerPrefs.Save();
            _playerId = id;
            Debug.Log("generate new Player Id " + _playerId);
            return _playerId;
        }

        public void Connect()
        {
            Debug.LogError("NetworkManager Connect is Called");
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
                Debug.LogError("[NetworkManager] Connected");
                _wasConnectedBefore = true;
                HideReconnectingOverlay();
                if (_client.IsInRoom)
                    _client.Reconnect();
            };

            _client.OnDisconnected += (reason) =>
            {
                Debug.Log($"[NetworkManager] Disconnected: {reason}");
                ShowReconnectingOverlay();
            };

            _client.OnError += (msg) =>
            {
                Debug.LogError($"[NetworkManager] Error: {msg}");
            };

            _client.OnRoomCreated += (roomId, player) =>
            {
                Debug.Log($"[NetworkManager] Room created: {roomId}");
            };

            _client.OnRoomJoined += (roomId, player) =>
            {
                Debug.Log($"[NetworkManager] Room joined: {roomId}");
            };

            _client.OnPlayerJoined += (payload) =>
            {
                Debug.Log($"[NetworkManager] Player joined: {payload.playerId}");
            };

            _client.OnPlayerLeft += (payload) =>
            {
                if (payload.playerId == _client.PlayerId)
                {
                    Debug.Log("[NetworkManager] Player left room");
                }
            };

            _client.OnGameStarted += () =>
            {
                Debug.Log("[NetworkManager] Game started");
            };

            _client.OnPlayerDisconnected += (playerId) =>
            {
                Debug.Log($"[NetworkManager] Player disconnected: {playerId}");
            };

            _client.OnPlayerReconnected += (playerId) =>
            {
                Debug.Log($"[NetworkManager] Player reconnected: {playerId}");
            };

            _client.OnRoomState += (payload) =>
            {
                HideReconnectingOverlay();
                Debug.Log("[NetworkManager] Room state restored");
            };
        }

        private void ShowReconnectingOverlay()
        {
            if (_reconnectOverlayShowing) return;
            _reconnectOverlayShowing = true;
            Debug.Log("[NetworkManager] Showing reconnecting overlay");
        }

        private void HideReconnectingOverlay()
        {
            if (!_reconnectOverlayShowing) return;
            _reconnectOverlayShowing = false;
            Debug.Log("[NetworkManager] Hiding reconnecting overlay");
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            if (!hasFocus && _client.IsConnected)
            {
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
}
