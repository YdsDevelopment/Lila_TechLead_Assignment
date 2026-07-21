using System;
using System.Collections.Generic;

namespace TicTacToe
{
    // --- Client → Server Payloads ---

    [Serializable]
    public class CreateRoomPayload
    {
        public string playerId;
    }

    [Serializable]
    public class JoinRoomPayload
    {
        public string roomId;
        public string playerId;
    }

    [Serializable]
    public class MakeMovePayload
    {
        public string roomId;
        public string playerId;
        public int row;
        public int col;
    }

    [Serializable]
    public class ReconnectPayload
    {
        public string playerId;
    }

    [Serializable]
    public class PlayAgainPayload
    {
        public string roomId;
        public string playerId;
    }

    [Serializable]
    public class LeaveRoomPayload
    {
        public string roomId;
        public string playerId;
    }

    // --- Server → Client Payloads ---

    [Serializable]
    public class RoomCreatedPayload
    {
        public string roomId;
        public PlayerSummary player;
    }

    [Serializable]
    public class RoomJoinedPayload
    {
        public string roomId;
        public PlayerSummary player;
    }

    [Serializable]
    public class PlayerJoinedPayload
    {
        public string playerId;
        public PlayerSymbol symbol;
        public List<PlayerSummary> players;
    }

    [Serializable]
    public class GameStartedPayload
    {
        public List<List<string>> board;
        public PlayerSummary currentPlayer;
        public long turnDeadline;
        public PlayerSummary playerX;
        public PlayerSummary playerO;
    }

    [Serializable]
    public class MoveResultPayload
    {
        public bool success;
        public string error;
        public MoveSummary move;
        public List<List<string>> board;
        public PlayerSummary currentPlayer;
        public long? turnDeadline;
        public PlayerSummary winner;
        public List<List<int>> winningCells;
        public bool isDraw;
        public bool timeoutWin;
    }

    [Serializable]
    public class TurnTimerPayload
    {
        public string roomId;
        public long remainingMs;
        public long turnDeadline;
        public PlayerSummary currentPlayer;
    }

    [Serializable]
    public class PlayerDisconnectedPayload
    {
        public string playerId;
    }

    [Serializable]
    public class PlayerReconnectedPayload
    {
        public string playerId;
    }

    [Serializable]
    public class PlayerLeftPayload
    {
        public string playerId;
        public string roomId;
        public int remainingPlayers;
        public string roomStatus;
    }

    [Serializable]
    public class RoomStatePayload
    {
        public string roomId;
        public string status;
        public List<PlayerSummary> players;
        public List<List<string>> board;
        public PlayerSummary currentPlayer;
        public long? turnDeadline;
        public PlayerSummary winner;
        public List<List<int>> winningCells;
        public bool isDraw;
        public List<MoveSummary> moves;
    }

    [Serializable]
    public class ErrorPayload
    {
        public string message;
    }
}
