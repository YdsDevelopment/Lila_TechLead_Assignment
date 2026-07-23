using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TicTacToe.UI
{
    public class LobbyUIHandler : MonoBehaviour
    {
        [SerializeField] private TMP_InputField _playerId;
        [SerializeField] private TMP_InputField _roomIDDisplay;
        [SerializeField] private TextMeshProUGUI _errorText;
        [SerializeField] private Button _createRoomButton;
        [SerializeField] private Button _reconnectToGameButton;
        [SerializeField] private Button _newPlayerIDButton;
        [SerializeField] private ScrollRect _roomListScrollRect;
        [SerializeField] private RectTransform _roomListContent;
        [SerializeField] private GameObject _roomItemPrefab;
        [SerializeField] private Image _overlayBlock;

        private Coroutine _pollRoomsCoroutine;
        private readonly Dictionary<string, RoomItem> _activeItems = new Dictionary<string, RoomItem>();
        private readonly Stack<RoomItem> _pool = new Stack<RoomItem>();

        private void Start()
        {
            InitialiseClickMethods();
            ToggleOverlay(false);
            RegisterNetworkEvents();
            if(_playerId != null)
                _playerId.text = NetworkManager.Instance.GetOrCreatePlayerId();
            enableDisConnectStateUI();
        }

        private void InitialiseClickMethods()
        {
            if (_createRoomButton != null)
                _createRoomButton.onClick.AddListener(OnCreateRoomClicked);

            if (_reconnectToGameButton != null)
                _reconnectToGameButton.onClick.AddListener(OnReconnectClicked);

            if (_newPlayerIDButton != null)
                _newPlayerIDButton.onClick.AddListener(generateNewPlayerID);
        }

        private void RegisterNetworkEvents()
        {
            // if (NetworkManager.Instance == null) return;
            NetworkManager.Instance.Client.OnConnected += OnConnected;
            NetworkManager.Instance.Client.OnDisconnected += OnDisconnected;
        }

        private void OnCreateRoomClicked()
        {
            if (NetworkManager.Instance?.Client == null || !NetworkManager.Instance.Client.IsConnected) return;
            NetworkManager.Instance.Client.CreateRoom();
        }

        // private void OnJoinRoomClicked()
        // {
        //     if (NetworkManager.Instance?.Client == null || !NetworkManager.Instance.Client.IsConnected) return;
        //     var roomId = _joinRoomInput != null ? _joinRoomInput.text.Trim() : "";
        //     if (string.IsNullOrEmpty(roomId)) return;
        //     NetworkManager.Instance.Client.JoinRoom(roomId);
        // }

        private void OnReconnectClicked()
        {
            // if (NetworkManager.Instance == null) return;
            NetworkManager.Instance.Client.Reconnect();
        }

        private void generateNewPlayerID()
        {
            // if (NetworkManager.Instance == null) return;
           var id = NetworkManager.Instance.RegeratePlayerID();
           if(_playerId != null)
                _playerId.text = id;
        }

        private void OnConnected()
        {
            enableConnectStateUI();
            StartPollingRooms();
        }

        private void OnDisconnected(string reason)
        {
            StopPollingRooms();
            enableDisConnectStateUI();
            ReleaseAllItems();
        }

        private void StartPollingRooms()
        {
            if (_pollRoomsCoroutine != null)
                StopCoroutine(_pollRoomsCoroutine);
            _pollRoomsCoroutine = StartCoroutine(PollRoomsRoutine());
        }

        private void StopPollingRooms()
        {
            if (_pollRoomsCoroutine != null)
            {
                StopCoroutine(_pollRoomsCoroutine);
                _pollRoomsCoroutine = null;
            }
        }

        private IEnumerator PollRoomsRoutine()
        {
            var wait = new WaitForSeconds(1.0f);
            while (true)
            {
                if (NetworkManager.Instance?.Client != null && NetworkManager.Instance.Client.IsConnected)
                    NetworkManager.Instance.Client.RequestRooms();
                yield return wait;
            }
        }

        private void OnEnable()
        {
            if (NetworkManager.Instance?.Client != null){
                NetworkManager.Instance.Client.OnRoomsList += OnRoomsListReceived;
                NetworkManager.Instance.Client.OnRoomCreated += OnRoomDetailsReceived;
                NetworkManager.Instance.Client.OnRoomJoined += OnRoomJoinDetailsReceived;
                NetworkManager.Instance.Client.OnError += OnNetworkError;
            }
        }

        private void OnDisable()
        {
            StopPollingRooms();
            if (NetworkManager.Instance?.Client != null) {
                NetworkManager.Instance.Client.OnRoomsList -= OnRoomsListReceived;
                NetworkManager.Instance.Client.OnRoomCreated -= OnRoomDetailsReceived;
                NetworkManager.Instance.Client.OnRoomJoined -= OnRoomJoinDetailsReceived;
                NetworkManager.Instance.Client.OnError += OnNetworkError;
            }
        }

        private void OnRoomsListReceived(RoomsListPayload payload)
        {
            SyncRoomList(payload.rooms);
        }

        private void OnNetworkError(string error)
        {
            if (_errorText)
            {
                _errorText.text = error;
            }
        }

        private void OnRoomDetailsReceived(string roomId, PlayerSummary payload)
        {
            if(payload != null)
            {
                _roomIDDisplay.text = roomId;
                ToggleOverlay(true);
                _errorText.text = "waiting for the Player..";
            }
        }

        private void OnRoomJoinDetailsReceived(string roomId, PlayerSummary payload)
        {
            if(payload != null)
            {
                _roomIDDisplay.text = roomId;
                ToggleOverlay(true);
                _errorText.text = "Joined Room, waiting for Game start..";
            }
        }

        private void SyncRoomList(List<RoomSummary> rooms)
        {
            if (_roomListContent == null || _roomItemPrefab == null) return;

            var incomingIds = rooms != null
                ? new HashSet<string>(rooms.Select(r => r.roomId))
                : new HashSet<string>();

            var existingIds = new HashSet<string>(_activeItems.Keys);

            foreach (var id in existingIds)
            {
                if (!incomingIds.Contains(id))
                {
                    ReturnToPool(_activeItems[id]);
                    _activeItems.Remove(id);
                }
            }

            if (rooms == null) return;

            foreach (var room in rooms)
            {
                if (_activeItems.ContainsKey(room.roomId))
                {
                    _activeItems[room.roomId].Setup(room.roomId, room.playerCount);
                }
                else
                {
                    var item = GetFromPool();
                    Debug.LogError("Prefab Item: is Presnt : " + item == null);
                    Debug.LogError("Prefab Item: is Presnt :  room : " + room == null);
                    
                    item.Setup(room.roomId, room.playerCount);
                    item.transform.SetAsLastSibling();
                    _activeItems[room.roomId] = item;
                }
            }
        }

        private RoomItem GetFromPool()
        {
            if (_pool.Count > 0)
            {
                var item = _pool.Pop();
                item.transform.SetParent(_roomListContent, false);
                return item;
            }
            var go = Instantiate(_roomItemPrefab, _roomListContent);
            return go.GetComponent<RoomItem>();
        }

        private void ReturnToPool(RoomItem item)
        {
            item.Release();
            item.transform.SetParent(null);
            _pool.Push(item);
        }

        private void ReleaseAllItems()
        {
            foreach (var kvp in _activeItems)
                ReturnToPool(kvp.Value);
            _activeItems.Clear();
        }

        private void ToggleOverlay(bool isActive )
        {
            if (_overlayBlock)
            {
                _overlayBlock.gameObject.SetActive(isActive);
            }
        }

        private void enableConnectStateUI()
        {
            _createRoomButton.interactable = true;
            _reconnectToGameButton.interactable = true;
        }

        private void enableDisConnectStateUI()
        {
            _createRoomButton.interactable = false;
            _reconnectToGameButton.interactable = false;
            _roomIDDisplay.text = "";
            _errorText.text = "";
            ToggleOverlay(false);
        }

        public void ResetLobbyUI()
        {
            _roomIDDisplay.text = "";
            _playerId.text = "";
            _errorText.text="";
            _newPlayerIDButton.gameObject.SetActive(true);
            _createRoomButton.interactable = false;
            _reconnectToGameButton.interactable = false;
            ToggleOverlay(false);
        }
    }
}
