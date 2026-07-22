using System;
using System.Collections.Generic;

namespace TicTacToe
{
    [Serializable]
    public class Player
    {
        public string playerId;
        public string socketId;
        public PlayerSymbol symbol;
        public bool connected;
        public DateTime joinedAt;
    }

    [Serializable]
    public class Room
    {
        public string roomId;
        public RoomStatus status;
        public List<Player> players;
        public DateTime createdAt;
        public DateTime updatedAt;
    }

    [Serializable]
    public class Move
    {
        public string playerId;
        public int row;
        public int col;
        public DateTime timestamp;
    }

    [Serializable]
    public class GameState
    {
        public string[,] board;
        public Player currentPlayer;
        public List<Move> moves;
        public Player winner;
        public bool isDraw;
        public DateTime startedAt;
        public DateTime? completedAt;
    }

    [Serializable]
    public class PlayerSummary
    {
        public string playerId;
        public PlayerSymbol symbol;
        public bool connected;
    }

    [Serializable]
    public class MoveSummary
    {
        public string playerId;
        public PlayerSymbol symbol;
        public int row;
        public int col;
        public string timestamp;
    }

    [Serializable]
    public class RoomSummary
    {
        public string roomId;
        public string status;
        public int playerCount;
        public List<PlayerSummary> players;
        public DateTime createdAt;
        public DateTime updatedAt;
    }

    [Serializable]
    public class HealthResponse
    {
        public string status;
        public string server;
    }

    [Serializable]
    public class ErrorResponse
    {
        public string message;
        public string error;
    }

    [Serializable]
    public class GameSnapshot
    {
        public List<List<string>> board;
        public PlayerSummary currentPlayer;
        public long? turnDeadline;
        public PlayerSummary winner;
        public List<List<int>> winningCells;
        public bool isDraw;
        public List<MoveSummary> moves;
        public DateTime? startedAt;
        public DateTime? completedAt;
    }

    [Serializable]
    public class RoomDetailsResponse
    {
        public string roomId;
        public string status;
        public List<PlayerSummary> players;
        public GameSnapshot game;
        public DateTime createdAt;
        public DateTime updatedAt;
    }
}
