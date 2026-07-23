using System;
using SocketIOClient.Transport;
using TicTacToe.Utils;
using UnityEngine;

namespace TicTacToe
{
    public class NetworkManager : MonoBehaviour
    {
        public static NetworkManager Instance { get; private set; }

        private TicTacToeSocketClient _client;
        public TicTacToeSocketClient Client => _client;

        private string _serverUrl = "http://localhost:5000";
        public string ServerUrl => _serverUrl;

        public void SetServerUrl(string url)
        {
            if (!string.IsNullOrEmpty(url))
                _serverUrl = url;
        }
        private bool _wasConnectedBefore;
        private bool _reconnectOverlayShowing;
        private bool _wasInRoom;

        private void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            _ = UnityMainThreadDispatcher.Instance;

            _wasInRoom = PlayerIdentity.LoadWasInRoom();
            ISocketManager socketManager;
#if UNITY_WEBGL && !UNITY_EDITOR
            socketManager = gameObject.AddComponent<WebGLSocketManager>();
#else
            socketManager = new SocketManager();
#endif
            _client = new TicTacToeSocketClient(socketManager);
            RegisterClientEvents();
        }

        private void Start()
        {
        }

        public string GetOrCreatePlayerId()
        {
            return PlayerIdentity.GetOrCreatePlayerId();
        }

        public string RegeratePlayerID()
        {
            PlayerIdentity.RegeneratePlayerId();
            var id = PlayerIdentity.GetOrCreatePlayerId();
            Debug.Log("generate new Player Id " + id);
            return id;
        }

        public string RegeneratePlayerID()
        {
            return RegeratePlayerID();
        }

        public void Connect()
        {
            Debug.Log("NetworkManager Connect is Called");
            _client.Connect(_serverUrl, PlayerIdentity.GetOrCreatePlayerId());
        }

        public void DisconnectAndCleanup()
        {
            if(_client != null)
            {
                if (_client.IsInRoom)
                    _client.LeaveRoom();
                _client.Disconnect();
            }
            PersistInRoom(false);
        }

        private void PersistInRoom(bool inRoom)
        {
            _wasInRoom = inRoom;
            PlayerIdentity.SaveWasInRoom(inRoom);
        }

        private void RegisterClientEvents()
        {
            _client.OnConnected += () =>
            {
                Debug.Log("[NetworkManager] Connected");
                _wasConnectedBefore = true;
                HideReconnectingOverlay();
                if (_wasInRoom)
                    _client.Reconnect();
            };

            _client.OnDisconnected += (reason) =>
            {
                Debug.Log($"[NetworkManager] Disconnected: {reason}");
                ShowReconnectingOverlay();
            };

            _client.OnError += (msg) =>
            {
                Debug.Log($"[NetworkManager] Error: {msg}");
            };

            _client.OnRoomCreated += (roomId, player) =>
            {
                Debug.Log($"[NetworkManager] Room created: {roomId}");
                PersistInRoom(true);
            };

            _client.OnRoomJoined += (roomId, player) =>
            {
                Debug.Log($"[NetworkManager] Room joined: {roomId}");
                PersistInRoom(true);
            };

            _client.OnPlayerJoined += (payload) =>
            {
                Debug.Log($"[NetworkManager] Player joined: {payload.playerId}");
            };

            _client.OnPlayerLeft += (payload) =>
            {
                Debug.Log("[NetworkManager] Player left room " + JsonHelper.ToJsonString(payload));
                if (payload.playerId == _client.PlayerId)
                {
                    Debug.Log("[NetworkManager] Player left room");
                    PersistInRoom(false);
                }
            };

            _client.OnGameStarted += (payload) =>
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

        // private void OnApplicationFocus(bool hasFocus)
        // {
        //     if (!hasFocus && _client.IsConnected)
        //     {
        //     }
        //     else if (hasFocus && !_client.IsConnected && _wasConnectedBefore)
        //     {
        //         ShowReconnectingOverlay();
        //         Connect();
        //     }
        // }

        private void OnApplicationQuit()
        {
            // DisconnectAndCleanup();
        }
    }
}
