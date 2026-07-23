using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TicTacToe.UI{
    public class PlayerInfoUI : MonoBehaviour
    {
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        [SerializeField] private TextMeshProUGUI _playerLabel;
        [SerializeField] private Image _timerImage;

        private string _playerId;
        private string _playerSymbol;
        void Awake()
        {
            Reset();
        }

        public void Reset()
        {
            if(_playerLabel)
                _playerLabel.text = "";
            
            if(_timerImage)
                _timerImage.fillAmount = 1;

            _playerId = "";
            _playerSymbol = "";
        }

        public void InitialisePlayerDetails(string playerID, string playerSymbol)
        {
            Reset();
            _playerId = playerID;
            _playerSymbol = playerSymbol;
            if(_playerLabel)
                _playerLabel.text = _playerSymbol;
        }

        public void UpdateTimer(string playerID,float value)
        {
            if(_playerId != playerID){
                _timerImage.fillAmount = 0;
                return;
            }
            if(_timerImage)
                _timerImage.fillAmount = Mathf.Clamp01(value);
        }
    }
}

