using UnityEngine;

namespace TicTacToe.UI
{
    public class GameUIHandler : MonoBehaviour
    {
        [SerializeField] private GameObject _lobbyPanel;
        [SerializeField] private GameObject _gamePanel;
        [SerializeField] private GameObject _playAgainPopup;

        private void Start()
        {
            if (GameManager.Instance != null)
                GameManager.Instance.OnStateChanged += OnStateChanged;

            SetActivePanel(GameManager.Instance != null ? GameManager.Instance.CurrentState : GameUIState.Lobby);
        }

        private void OnDestroy()
        {
            if (GameManager.Instance != null)
                GameManager.Instance.OnStateChanged -= OnStateChanged;
        }

        private void OnStateChanged(GameUIState state)
        {
            SetActivePanel(state);
        }

        private void SetActivePanel(GameUIState state)
        {
            if (_lobbyPanel != null)
                _lobbyPanel.SetActive(state == GameUIState.Lobby);

            if (_gamePanel != null)
                _gamePanel.SetActive(state == GameUIState.Playing);
        }
    }
}
