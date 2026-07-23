using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TicTacToe.UI{

public enum connectionState
{
    CONNECTING,
    CONNECTED,
    DISCONNECTING,
    DISCONNECTED,
}

public class ConnectionStatusHandler : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [SerializeField] private Image _connectionDot;
    [SerializeField] private TextMeshProUGUI _text;

    [SerializeField] private Color _connectedColor;
    [SerializeField] private Color _disConnectedColor;
    [SerializeField] private Color _connectingColor;
    [SerializeField] private Button _connectButton;
    [SerializeField] private Button _disconnectButton;

    private connectionState _currentstate;
    void Start()
    {
        if(_connectionDot == null)
        {
            _connectionDot = GetComponentInChildren<Image>();
        }

        if(_text == null)
        {
            _text = GetComponentInChildren<TextMeshProUGUI>();
        }
        SetState(connectionState.DISCONNECTED);
    }

    private void RegisterNetworkEvents()
    {
        NetworkManager.Instance.Client.OnConnected += OnConnected;
        NetworkManager.Instance.Client.OnDisconnected += OnDisconnected;
    }

    private void InitialiseClickMethods()
    {
        if (_connectButton != null)
            _connectButton.onClick.AddListener(OnConnectClicked);

        if (_disconnectButton != null)
            _disconnectButton.onClick.AddListener(OnDisconnectClicked);
    }

    private void enableConnectStateUI()
    {
        _connectButton.gameObject.SetActive(false);
        _disconnectButton.gameObject.SetActive(true);
    }

    private void enableDisConnectStateUI()
    {
        _connectButton.gameObject.SetActive(true);
        _disconnectButton.gameObject.SetActive(false);
    }

    private void OnConnectClicked()
    {
        NetworkManager.Instance.Connect();
        _connectButton.gameObject.SetActive(false);
        SetState(connectionState.CONNECTING);
    }

    private void OnDisconnectClicked()
    {
        NetworkManager.Instance.DisconnectAndCleanup();
        SetState(connectionState.DISCONNECTING);
        _disconnectButton.gameObject.SetActive(false);
    }

    private void OnConnected()
    {
        SetState(connectionState.CONNECTED);
        enableConnectStateUI();
    }

    private void OnDisconnected(string reason)
    {
        SetState(connectionState.DISCONNECTED);
        enableDisConnectStateUI();
    }

    public void SetState(connectionState state)
    {
        _currentstate = state;
        switch (state)
        {
            case connectionState.CONNECTED:
                if (_connectionDot != null)
                    _connectionDot.color = _connectedColor;
                if (_text != null)
                    _text.text = "Connected";
                break;
            case connectionState.DISCONNECTED:
                if (_connectionDot != null)
                    _connectionDot.color = _disConnectedColor;
                if (_text != null)
                    _text.text = "Disconnected";
                break;
            case connectionState.CONNECTING:
                if (_connectionDot != null)
                    _connectionDot.color = _disConnectedColor;
                if (_text != null)
                    _text.text = "Connecting..";
                break;
            case connectionState.DISCONNECTING:
                if (_connectionDot != null)
                    _connectionDot.color = _disConnectedColor;
                if (_text != null)
                    _text.text = "Disconnecting..";
                break;
        }
    }
}
}
