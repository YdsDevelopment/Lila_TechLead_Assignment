using System;

namespace TicTacToe
{
    public interface ISocketManager
    {
        bool IsConnected { get; }

        void Connect(string serverUrl);
        void Disconnect();

        void Emit(string eventName, object payload);

        event Action OnConnected;
        event Action<string> OnDisconnected;
        event Action<string> OnConnectError;

        event Action<RoomCreatedPayload> OnRoomCreated;
        event Action<RoomJoinedPayload> OnRoomJoined;
        event Action<PlayerJoinedPayload> OnPlayerJoined;
        event Action<GameStartedPayload> OnGameStarted;
        event Action<MoveResultPayload> OnMoveResult;
        event Action<TurnTimerPayload> OnTurnTimer;
        event Action<PlayerDisconnectedPayload> OnPlayerDisconnected;
        event Action<PlayerReconnectedPayload> OnPlayerReconnected;
        event Action<PlayerLeftPayload> OnPlayerLeft;
        event Action<RoomStatePayload> OnRoomState;
        event Action<ErrorPayload> OnError;
    }
}
