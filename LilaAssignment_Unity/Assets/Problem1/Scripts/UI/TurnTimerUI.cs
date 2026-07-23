using UnityEngine;
using UnityEngine.UI;

namespace TicTacToe.UI
{
    public class TurnTimerUI : MonoBehaviour
    {
        [SerializeField] private Slider _timerSlider;
        [SerializeField] private Text _timerText;
        [SerializeField] private Image _timerFill;

        private long _totalDuration;

        private void Start()
        {
            if (NetworkManager.Instance != null)
            {
                NetworkManager.Instance.Client.OnTurnTimer += OnTurnTimer;
                NetworkManager.Instance.Client.OnGameStarted += Reset;
            }
        }

        private void OnTurnTimer(string playerId,long remainingMs, long turnDeadline)
        {
            if (_totalDuration == 0)
                _totalDuration = remainingMs;

            float pct = _totalDuration > 0 ? Mathf.Clamp01((float)remainingMs / _totalDuration) : 0;

            if (_timerSlider != null)
                _timerSlider.value = pct;

            if (_timerText != null)
                _timerText.text = $"{(remainingMs / 1000f):F1}s";

            if (_timerFill != null)
            {
                if (pct < 0.2f)
                    _timerFill.color = Color.red;
                else if (pct < 0.4f)
                    _timerFill.color = Color.yellow;
                else
                    _timerFill.color = Color.green;
            }
        }

        public void Reset(GameStartedPayload payload)
        {
            _totalDuration = 0;
            if (_timerSlider != null)
                _timerSlider.value = 1f;
            if (_timerText != null)
                _timerText.text = "-";
        }
    }
}
