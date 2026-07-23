using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

namespace TicTacToe.UI
{
    public class GameUIHandler : MonoBehaviour
    {
        [SerializeField] private LobbyUIHandler _lobbyPanel;
        [SerializeField] private InGamePanelUIHandler _gamePanel;
        [SerializeField] private PlayAgainPanel _playAgainPopup;

        private Coroutine _showGameOverCoroutine;

        private void Start()
        {
            if (GameManager.Instance != null)
                GameManager.Instance.OnStateChanged += OnStateChanged;

            SetActivePanel(GameManager.Instance != null ? GameManager.Instance.CurrentState : GameUIState.Lobby, false);
        }

        private void OnDestroy()
        {
            if (GameManager.Instance != null)
                GameManager.Instance.OnStateChanged -= OnStateChanged;
        }

        private void OnStateChanged(GameUIState state, bool isTimeOutWin)
        {
            SetActivePanel(state, isTimeOutWin);
        }

        private void SetActivePanel(GameUIState state, bool isTimeOutWin)
        {
            Debug.LogError("Game State Change : " + state);
            switch (state)
            {
                case GameUIState.Lobby:
                    StopShowGameOverCoroutine();
                    if (_lobbyPanel != null)
                        _lobbyPanel.gameObject.SetActive(state == GameUIState.Lobby);
                    if (_gamePanel != null)
                        _gamePanel.Hide();
                break;
                case GameUIState.Playing:
                    StopShowGameOverCoroutine();
                    if (_gamePanel != null)
                        _gamePanel.Show();
                    if (_lobbyPanel != null)
                        _lobbyPanel.gameObject.SetActive(state == GameUIState.Lobby);
                break;
                case GameUIState.GameOver:
                    StopShowGameOverCoroutine();
                    _showGameOverCoroutine = StartCoroutine(ShowGameOver(isTimeOutWin));
                break;
            }
        }

        private IEnumerator ShowGameOver(bool isTimeoutWin)
        {
            if (isTimeoutWin == false)
            {
                yield return new WaitForSeconds(6.0f);
            }
            if (_gamePanel != null)
                _gamePanel.Hide();

            _showGameOverCoroutine = null;
        }

        private void StopShowGameOverCoroutine()
        {
            if (_showGameOverCoroutine == null) return;

            StopCoroutine(_showGameOverCoroutine);
            _showGameOverCoroutine = null;
        }
    }
}
