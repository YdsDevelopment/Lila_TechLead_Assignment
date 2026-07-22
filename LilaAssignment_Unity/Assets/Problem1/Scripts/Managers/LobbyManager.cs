using System;
using System.Collections.Generic;
using UnityEngine;

namespace TicTacToe
{
    public class LobbyManager : MonoBehaviour
    {
        public static LobbyManager Instance { get; private set; }

        public event Action<List<RoomSummary>> OnRoomsUpdated;
        public event Action<string> OnFetchError;
        public event Action<HealthStatusPayload> OnHealthReceived;
        public event Action<RoomDetailsPayload> OnRoomDetailsReceived;

        public List<RoomSummary> CachedRooms { get; private set; }
        public bool IsFetching { get; private set; }

        private TicTacToeSocketClient _client;

        private string _roomId;

        private void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            _client = NetworkManager.Instance.Client;
            RegisterHandlers();
        }

        private void RegisterHandlers()
        {
            _client.OnRoomsList += (payload) =>
            {
                CachedRooms = payload.rooms ?? new List<RoomSummary>();
                OnRoomsUpdated?.Invoke(CachedRooms);
                IsFetching = false;
            };

            _client.OnRoomDetails += (payload) =>
            {
                OnRoomDetailsReceived?.Invoke(payload);
            };

            _client.OnHealthStatus += (payload) =>
            {
                OnHealthReceived?.Invoke(payload);
            };

            _client.OnError += (msg) =>
            {
                OnFetchError?.Invoke(msg);
            };

            _client.OnRoomCreated += (roomId,payload) =>
            {
                OnRoomDetails(roomId, payload);
            };

            _client.OnRoomJoined += (roomId,payload) =>
            {
                OnRoomDetails(roomId, payload);
            };
        }

        public void RefreshRooms()
        {
            if (IsFetching) return;
            IsFetching = true;
            _client.RequestRooms();
        }

        public void GetRoomById(string roomId)
        {
            if (string.IsNullOrWhiteSpace(roomId))
            {
                OnFetchError?.Invoke("Room ID is required");
                return;
            }
            _client.RequestRoom(roomId);
        }

        public void GetHealth()
        {
            _client.RequestHealth();
        }

        private void OnRoomDetails(string roomId , PlayerSummary payload)
        {
            if (payload != null)
            {
                _roomId = roomId;
                PlayerPrefs.SetString("RoomId",_roomId);
                PlayerPrefs.Save();
            }
        }
    }
}
