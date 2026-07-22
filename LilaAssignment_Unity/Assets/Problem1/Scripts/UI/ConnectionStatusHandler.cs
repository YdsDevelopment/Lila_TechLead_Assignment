using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TicTacToe.UI{

public enum connectionState
{
    CONNECTING,
    CONNECTED,
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
        }
    }
}
}
