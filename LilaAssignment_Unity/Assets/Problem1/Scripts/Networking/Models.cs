using System;
using System.Collections.Generic;

namespace TicTacToe
{
    [Serializable]
    public class Player
    {
        public string playerId { get; set; }
        public string socketId { get; set; }
        public PlayerSymbol symbol { get; set; }
        public bool connected { get; set; }
        public DateTime joinedAt { get; set; }
    }

    [Serializable]
    public class Room
    {
        public string roomId { get; set; }
        public RoomStatus status { get; set; }
        public List<Player> players { get; set; }
        public DateTime createdAt { get; set; }
        public DateTime updatedAt { get; set; }
    }

    [Serializable]
    public class Move
    {
        public string playerId { get; set; }
        public int row { get; set; }
        public int col { get; set; }
        public DateTime timestamp { get; set; }
    }

    [Serializable]
    public class GameState
    {
        public string[,] board { get; set; }
        public Player currentPlayer { get; set; }
        public List<Move> moves { get; set; }
        public Player winner { get; set; }
        public bool isDraw { get; set; }
        public DateTime startedAt { get; set; }
        public DateTime? completedAt { get; set; }
    }

    [Serializable]
    public class PlayerSummary
    {
        public string playerId { get; set; }
        public PlayerSymbol symbol { get; set; }
        public bool connected { get; set; }
    }

    [Serializable]
    public class MoveSummary
    {
        public string playerId { get; set; }
        public PlayerSymbol symbol { get; set; }
        public int row { get; set; }
        public int col { get; set; }
        public string timestamp { get; set; }
    }

    [Serializable]
    public class RoomSummary
    {
        public string roomId { get; set; }
        public string status { get; set; }
        public int playerCount { get; set; }
        public List<PlayerSummary> players { get; set; }
        public DateTime createdAt { get; set; }
        public DateTime updatedAt { get; set; }
    }

    [Serializable]
    public class HealthResponse
    {
        public string status { get; set; }
        public string server { get; set; }
    }

    [Serializable]
    public class ErrorResponse
    {
        public string message { get; set; }
        public string error { get; set; }
    }

    [Serializable]
    public class GameSnapshot
    {
        public List<List<string>> board { get; set; }
        public PlayerSummary currentPlayer { get; set; }
        public long? turnDeadline { get; set; }
        public PlayerSummary winner { get; set; }
        public List<List<int>> winningCells { get; set; }
        public bool isDraw { get; set; }
        public List<MoveSummary> moves { get; set; }
        public DateTime? startedAt { get; set; }
        public DateTime? completedAt { get; set; }
    }

    [Serializable]
    public class RoomDetailsResponse
    {
        public string roomId { get; set; }
        public string status { get; set; }
        public List<PlayerSummary> players { get; set; }
        public GameSnapshot game { get; set; }
        public DateTime createdAt { get; set; }
        public DateTime updatedAt { get; set; }
    }
}
