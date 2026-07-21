export const SocketEvents = {
  CREATE_ROOM: "create-room",
  JOIN_ROOM: "join-room",
  MAKE_MOVE: "make-move",
  RECONNECT: "reconnect",
  DISCONNECT: "disconnect",
  ROOM_CREATED: "room-created",
  PLAYER_JOINED: "player-joined",
  GAME_STARTED: "game-started",
  MOVE_RESULT: "move-result",
  PLAYER_DISCONNECTED: "player-disconnected",
  PLAYER_RECONNECTED: "player-reconnected",
  ROOM_STATE: "room-state",
  TURN_TIMER: "turn-timer",
  ERROR: "error",
} as const;

export type SocketEventType = (typeof SocketEvents)[keyof typeof SocketEvents];
