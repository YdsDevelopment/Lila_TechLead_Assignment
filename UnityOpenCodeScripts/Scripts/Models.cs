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
}
