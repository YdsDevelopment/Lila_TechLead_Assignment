using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace TicTacToe
{
    public static class PlayerIdentity
    {
        private const string PlayerIdPrefsKey = "playerId";

#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")] private static extern string TTT_SessionStorageGetItem(string key);
        [DllImport("__Internal")] private static extern void TTT_SessionStorageSetItem(string key, string value);
#endif

        private const string WasInRoomPrefsKey = "was_in_room";

        private static string _cachedPlayerId;

        public static string GetOrCreatePlayerId()
        {
            if (!string.IsNullOrEmpty(_cachedPlayerId))
                return _cachedPlayerId;

            string id = LoadPlayerId();
            if (string.IsNullOrEmpty(id))
            {
                id = Guid.NewGuid().ToString();
                SavePlayerId(id);
            }

            _cachedPlayerId = id;
            return id;
        }

        public static void RegeneratePlayerId()
        {
            var id = Guid.NewGuid().ToString();
            SavePlayerId(id);
            _cachedPlayerId = id;
        }

        private static string LoadPlayerId()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            return TTT_SessionStorageGetItem(PlayerIdPrefsKey) ?? string.Empty;
#else
            return PlayerPrefs.GetString(PlayerIdPrefsKey, string.Empty);
#endif
        }

        private static void SavePlayerId(string id)
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            TTT_SessionStorageSetItem(PlayerIdPrefsKey, id);
#else
            PlayerPrefs.SetString(PlayerIdPrefsKey, id);
            PlayerPrefs.Save();
#endif
        }

        public static bool LoadWasInRoom()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            var value = TTT_SessionStorageGetItem(WasInRoomPrefsKey);
            return value == "1";
#else
            return PlayerPrefs.GetInt(WasInRoomPrefsKey, 0) == 1;
#endif
        }

        public static void SaveWasInRoom(bool inRoom)
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            TTT_SessionStorageSetItem(WasInRoomPrefsKey, inRoom ? "1" : "0");
#else
            PlayerPrefs.SetInt(WasInRoomPrefsKey, inRoom ? 1 : 0);
            PlayerPrefs.Save();
#endif
        }
    }
}
