using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TicTacToe.UI
{
    public class PlayAgainPanel : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _winnerText;
        [SerializeField] private Button _playAgainButton;
        [SerializeField] private Button _exitButton;
        [SerializeField] private float _delayDuration = 5.5f;

        [SerializeField] private GameObject _bg;
        [SerializeField] private GameObject _popup;

        private TicTacToeSocketClient _client;
        private MoveResultPayload _lastMoveResult;

        private void Start()
        {
            _client = NetworkManager.Instance.Client;

            _client.OnMoveResult += OnMoveResult;
            GameManager.Instance.OnPlayAgainReady += OnPlayAgainReady;
            GameManager.Instance.OnGameStarted += Hide;
            GameManager.Instance.OnReturnToLobby += Hide;

            _playAgainButton.onClick.AddListener(OnPlayAgainClicked);
            _exitButton.onClick.AddListener(OnExitClicked);

            _bg.SetActive(false);
            _popup.SetActive(false);
        }

        private void OnDestroy()
        {
            if (_client != null)
                _client.OnMoveResult -= OnMoveResult;

            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnPlayAgainReady -= OnPlayAgainReady;
                GameManager.Instance.OnGameStarted -= Hide;
                GameManager.Instance.OnReturnToLobby -= Hide;
            }
        }

        private void OnMoveResult(MoveResultPayload payload)
        {
            _lastMoveResult = payload;
        }

        private void OnPlayAgainReady()
        {
            StartCoroutine(ShowAfterDelay());
        }

        private IEnumerator ShowAfterDelay()
        {
            yield return new WaitForSeconds(_delayDuration);
            Show();
        }

        private void Show()
        {
            if (_winnerText != null && _lastMoveResult != null)
            {
                if (_lastMoveResult.timeoutWin)
                {
                    bool iWon = _lastMoveResult.winner != null &&
                                _lastMoveResult.winner.playerId == _client.PlayerId;
                    _winnerText.text = iWon ? "Opponent Timed Out!\nYou Win!" : "You Timed Out!";
                }
                else if (_lastMoveResult.winner != null)
                {
                    bool iWon = _lastMoveResult.winner.playerId == _client.PlayerId;
                    _winnerText.text = iWon ? "You Won!" : $"{_lastMoveResult.winner.symbol} Wins!";
                }
                else if (_lastMoveResult.isDraw)
                {
                    _winnerText.text = "It's a Draw!";
                }
            }

            _bg.SetActive(true);
            _popup.SetActive(true);
        }

        private void Hide()
        {
            StopAllCoroutines();
            _bg.SetActive(false);
            _popup.SetActive(false);
        }

        private void OnPlayAgainClicked()
        {
            GameManager.Instance.PlayAgain();
            Hide();
        }

        private void OnExitClicked()
        {
            GameManager.Instance.ExitSession();
            Hide();
        }
    }
}
