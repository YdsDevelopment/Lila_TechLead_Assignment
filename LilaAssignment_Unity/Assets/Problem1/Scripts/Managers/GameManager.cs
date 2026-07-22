using System;
using UnityEngine;

namespace TicTacToe
{
    public enum GameUIState
    {
        Lobby,
        Playing,
        GameOver
    }

    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        public GameUIState CurrentState { get; private set; } = GameUIState.Lobby;

        public event Action<GameUIState> OnStateChanged;
        public event Action OnGameStarted;
        public event Action OnReturnToLobby;
        public event Action OnPlayAgainReady;

        private TicTacToeSocketClient _client;

        private void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            _client = NetworkManager.Instance.Client;
            RegisterEvents();
            Debug.LogError("GameManager Connect is Called");
            NetworkManager.Instance.Connect();
        }

        private void RegisterEvents()
        {
            _client.OnRoomCreated += (roomId, player) =>
            {
                SetState(GameUIState.Lobby);
            };

            _client.OnRoomJoined += (roomId, player) =>
            {
                SetState(GameUIState.Lobby);
            };

            _client.OnGameStarted += () =>
            {
                SetState(GameUIState.Playing);
                OnGameStarted?.Invoke();
            };

            _client.OnMoveResult += (payload) =>
            {
                if (payload.winner != null || payload.isDraw || payload.timeoutWin)
                {
                    SetState(GameUIState.GameOver);
                    OnPlayAgainReady?.Invoke();
                }
            };

            _client.OnPlayerLeft += (payload) =>
            {
                if (payload.playerId == _client.PlayerId)
                {
                    ReturnToLobby();
                }
            };

            _client.OnDisconnected += (_) =>
            {
                SetState(GameUIState.Lobby);
            };
        }

        public void PlayAgain()
        {
            if (_client == null || !_client.IsInRoom) return;
            SetState(GameUIState.Playing);
            _client.PlayAgain();
        }

        public void ExitSession()
        {
            if (_client != null)
            {
                if (_client.IsInRoom)
                    _client.LeaveRoom();
            }
            ReturnToLobby();
        }

        private void ReturnToLobby()
        {
            SetState(GameUIState.Lobby);
            OnReturnToLobby?.Invoke();
        }

        private void SetState(GameUIState newState)
        {
            if (CurrentState == newState) return;
            CurrentState = newState;
            OnStateChanged?.Invoke(newState);
        }
    }
}
