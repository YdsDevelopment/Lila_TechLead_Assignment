using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace TicTacToe
{
    public class LobbyManager : MonoBehaviour
    {
        public static LobbyManager Instance { get; private set; }

        public event Action<List<RoomSummary>> OnRoomsUpdated;
        public event Action<string> OnFetchError;

        public List<RoomSummary> CachedRooms { get; private set; }
        public bool IsFetching { get; private set; }

        private void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public async Task RefreshRooms()
        {
            if (IsFetching) return;
            IsFetching = true;

            try
            {
                var rooms = await APIUtils.GetOpenRoomsAsync();
                CachedRooms = rooms.ToList();
                OnRoomsUpdated?.Invoke(CachedRooms);
            }
            catch (ApiException ex)
            {
                var message = string.IsNullOrEmpty(ex.Message) ? "Failed to fetch rooms" : ex.Message;
                Debug.LogError($"[LobbyManager] API error: {ex.StatusCode} - {message}");
                OnFetchError?.Invoke(message);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[LobbyManager] Failed to fetch rooms: {ex.Message}");
                OnFetchError?.Invoke(ex.Message);
            }
            finally
            {
                IsFetching = false;
            }
        }
    }
}
