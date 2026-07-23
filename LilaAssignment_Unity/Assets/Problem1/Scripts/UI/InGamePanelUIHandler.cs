using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TicTacToe.UI
{

    public class InGamePanelUIHandler : MonoBehaviour
    {
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        [SerializeField] private GameObject _mainGameObject;
        [SerializeField] private TextMeshProUGUI _errorText;

        [SerializeField] private PlayerInfoUI _player1;
        [SerializeField] private PlayerInfoUI _player2;

        private const float TURNTIME_IN_MS = 30000;
        void Awake()
        {
            if (NetworkManager.Instance != null)
            {
                Debug.LogError("InGamePanelUIHandler : NetUpdated");
                NetworkManager.Instance.Client.OnTurnTimer += updatePlayerTimer;
                NetworkManager.Instance.Client.OnGameStarted += OnGameStarted;
            }
            if(_mainGameObject)
                _mainGameObject.SetActive(false);

            if(GameManager.Instance != null)
            {
                GameManager.Instance.OnRoomStateRestored += OnRoomStateRestoredHandler;
            }
        }

        public void Show()
        {   
            Reset();
            if(_mainGameObject)
                _mainGameObject.SetActive(true);
        }

        public void Hide()
        {
           if(_mainGameObject)
                _mainGameObject.SetActive(false);
            Reset();
        }

        public void setErrorText(string assignedLabel)
        {
            if(_errorText)
                _errorText.text = assignedLabel;
        }

        public void setActivePlayer(string assignedLabel)
        {
            if(_errorText)
                _errorText.text = assignedLabel;
        }

        private void OnGameStarted(GameStartedPayload payload)
        {
            Debug.LogError("InGamePanelUIHandler OnGameStarted =>" + JsonHelper.ToJsonString(payload));
            if(payload != null)
            {
                if (payload.playerX != null)
                {
                    InitialisePlayers(0,payload.playerX.playerId, payload.playerX.symbol.ToString());
                }
                if (payload.playerO != null)
                {
                    InitialisePlayers(1,payload.playerO.playerId, payload.playerO.symbol.ToString());
                }
            }
        }

        private void OnRoomStateRestoredHandler(RoomStatePayload payload)
        {
            if (payload == null) return;
            
            if(payload.players != null)
            {
                for(int i = 0; i < payload.players.Count; i++)
                {
                    InitialisePlayers(i, payload.players[i].playerId, payload.players[i].symbol.ToString());
                }
            }
        }

        public void InitialisePlayers(int index, string playerID, string playerSymbol)
        {
            if(index == 0)
            {
                if(_player1)
                {
                    _player1.InitialisePlayerDetails(playerID,playerSymbol);
                }
            }
            else if(index == 1)
            {
                if(_player2)
                {
                    _player2.InitialisePlayerDetails(playerID,playerSymbol);
                }
            }
        }

        public void updatePlayerTimer(string playerId, long remaingTime, long turnDeadLine)
        {
            float percentage = TURNTIME_IN_MS > 0 ? Mathf.Clamp01((float)remaingTime / TURNTIME_IN_MS) : 0f;
            
            if(playerId == NetworkManager.Instance.GetOrCreatePlayerId())
            {
                _errorText.text = "Your Turn";
            }
            else
            {
                _errorText.text = "Opponent's Turn";
            }
            
            if(_player1)
                _player1.UpdateTimer(playerId,percentage);
            if(_player2)
                _player2.UpdateTimer(playerId,percentage);
        }

        public void Reset()
        {
            if(_errorText)
                _errorText.text = "";
            // if(_player1)
            //     _player1.Reset();
            // if(_player2)
            //     _player2.Reset();
        }

        public void OnDestroy()
        {
            Reset();
            if (NetworkManager.Instance != null)
            {
                NetworkManager.Instance.Client.OnTurnTimer -= updatePlayerTimer;
                NetworkManager.Instance.Client.OnGameStarted -= OnGameStarted;
            }

            if(GameManager.Instance != null)
            {
                GameManager.Instance.OnRoomStateRestored -= OnRoomStateRestoredHandler;
            }
        }
    }
}
