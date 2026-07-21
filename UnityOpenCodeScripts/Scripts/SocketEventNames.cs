namespace TicTacToe
{
    public static class SocketEventNames
    {
        // Client → Server
        public const string CREATE_ROOM = "create-room";
        public const string JOIN_ROOM = "join-room";
        public const string MAKE_MOVE = "make-move";
        public const string RECONNECT = "reconnect";
        public const string PLAY_AGAIN = "play-again";
        public const string LEAVE_ROOM = "leave-room";

        // Server → Client
        public const string ROOM_CREATED = "room-created";
        public const string ROOM_JOINED = "room-joined";
        public const string PLAYER_JOINED = "player-joined";
        public const string GAME_STARTED = "game-started";
        public const string MOVE_RESULT = "move-result";
        public const string TURN_TIMER = "turn-timer";
        public const string PLAYER_DISCONNECTED = "player-disconnected";
        public const string PLAYER_RECONNECTED = "player-reconnected";
        public const string PLAYER_LEFT = "player-left";
        public const string ROOM_STATE = "room-state";
        public const string ERROR = "error";
    }
}
