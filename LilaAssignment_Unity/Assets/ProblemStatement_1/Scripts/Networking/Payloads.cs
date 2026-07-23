using System;
using System.Collections.Generic;

namespace TicTacToe
{
    // --- Client → Server Payloads ---

    [Serializable]
    public class GetHealthPayload
    {
        public string playerId { get; set; }
    }

    [Serializable]
    public class GetRoomsPayload
    {
        public string playerId { get; set; }
    }

    [Serializable]
    public class GetRoomPayload
    {
        public string roomId { get; set; }
        public string playerId { get; set; }
    }

    [Serializable]
    public class CreateRoomPayload
    {
        public string playerId { get; set; }
    }

    [Serializable]
    public class JoinRoomPayload
    {
        public string roomId { get; set; }
        public string playerId { get; set; }
    }

    [Serializable]
    public class MakeMovePayload
    {
        public string roomId { get; set; }
        public string playerId { get; set; }
        public int row { get; set; }
        public int col { get; set; }
    }

    [Serializable]
    public class ReconnectPayload
    {
        public string playerId { get; set; }
    }

    [Serializable]
    public class PlayAgainPayload
    {
        public string roomId { get; set; }
        public string playerId { get; set; }
    }

    [Serializable]
    public class LeaveRoomPayload
    {
        public string roomId { get; set; }
        public string playerId { get; set; }
    }

    // --- Server → Client Payloads ---

    [Serializable]
    public class RoomCreatedPayload
    {
        public string roomId { get; set; }
        public PlayerSummary player { get; set; }
    }

    [Serializable]
    public class RoomJoinedPayload
    {
        public string roomId { get; set; }
        public PlayerSummary player { get; set; }
    }

    [Serializable]
    public class PlayerJoinedPayload
    {
        public string playerId { get; set; }
        public PlayerSymbol symbol { get; set; }
        public List<PlayerSummary> players { get; set; }
    }

    [Serializable]
    public class GameStartedPayload
    {
        public List<List<string>> board { get; set; }
        public PlayerSummary currentPlayer { get; set; }
        public long turnDeadline { get; set; }
        public PlayerSummary playerX { get; set; }
        public PlayerSummary playerO { get; set; }
    }

    [Serializable]
    public class MoveResultPayload
    {
        public bool success { get; set; }
        public string error { get; set; }
        public MoveSummary move { get; set; }
        public List<List<string>> board { get; set; }
        public PlayerSummary currentPlayer { get; set; }
        public long? turnDeadline { get; set; }
        public PlayerSummary winner { get; set; }
        public List<List<int>> winningCells { get; set; }
        public bool isDraw { get; set; }
        public bool timeoutWin { get; set; }
    }

    [Serializable]
    public class TurnTimerPayload
    {
        public string roomId { get; set; }
        public long remainingMs { get; set; }
        public long turnDeadline { get; set; }
        public PlayerSummary currentPlayer { get; set; }
    }

    [Serializable]
    public class PlayerDisconnectedPayload
    {
        public string playerId { get; set; }
    }

    [Serializable]
    public class PlayerReconnectedPayload
    {
        public string playerId { get; set; }
    }

    [Serializable]
    public class PlayerLeftPayload
    {
        public string playerId { get; set; }
        public string roomId { get; set; }
        public int remainingPlayers { get; set; }
        public string roomStatus { get; set; }
    }

    [Serializable]
    public class RoomStatePayload
    {
        public string roomId { get; set; }
        public string status { get; set; }
        public List<PlayerSummary> players { get; set; }
        public List<List<string>> board { get; set; }
        public PlayerSummary currentPlayer { get; set; }
        public long? turnDeadline { get; set; }
        public PlayerSummary winner { get; set; }
        public List<List<int>> winningCells { get; set; }
        public bool isDraw { get; set; }
        public List<MoveSummary> moves { get; set; }
    }

    [Serializable]
    public class ErrorPayload
    {
        public string message { get; set; }
    }

    [Serializable]
    public class HealthStatusPayload
    {
        public string status { get; set; }
        public string server { get; set; }
    }

    [Serializable]
    public class RoomsListPayload
    {
        public List<RoomSummary> rooms { get; set; }
    }

    [Serializable]
    public class RoomDetailsPayload
    {
        public string roomId { get; set; }
        public string status { get; set; }
        public List<PlayerSummary> players { get; set; }
        public GameSnapshot game { get; set; }
        public DateTime createdAt { get; set; }
        public DateTime updatedAt { get; set; }
    }
}
