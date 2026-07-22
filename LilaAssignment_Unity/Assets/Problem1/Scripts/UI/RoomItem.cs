using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TicTacToe.UI
{
    public class RoomItem : MonoBehaviour
    {
        [SerializeField] private TMP_InputField _roomIdText;
        [SerializeField] private TMP_InputField _playerCountText;
        [SerializeField] private Button _joinButton;

        private string _roomId;

        private void Awake()
        {
            if (_joinButton != null)
                _joinButton.onClick.AddListener(OnJoinClicked);
        }

        public void Setup(string roomId, int playerCount)
        {
            _roomId = roomId;
            gameObject.SetActive(true);
            if (_roomIdText != null)
                _roomIdText.text = roomId;
            if (_playerCountText != null)
                _playerCountText.text = playerCount + "/2";
        }

        public void Release()
        {
            _roomId = null;
            if (_joinButton != null)
                _joinButton.onClick.RemoveListener(OnJoinClicked);
            gameObject.SetActive(false);
        }

        private void OnJoinClicked()
        {
            Debug.LogError("OnJoinRoom Clicked: " + _roomId);
            if (NetworkManager.Instance?.Client != null && !string.IsNullOrEmpty(_roomId))
                NetworkManager.Instance.Client.JoinRoom(_roomId);
        }
    }
}
