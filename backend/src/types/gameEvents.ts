import { RoomStatus } from "../models/RoomStatus";

export interface ServerEvents {
  "room-created": (payload: RoomCreatedPayload) => void;
  "player-joined": (payload: PlayerJoinedPayload) => void;
  "game-started": (payload: GameStartedPayload) => void;
  "move-result": (payload: MoveResultPayload) => void;
  "player-disconnected": (payload: PlayerDisconnectedPayload) => void;
  "player-reconnected": (payload: PlayerReconnectedPayload) => void;
  "room-state": (payload: RoomStatePayload) => void;
  "error": (payload: ErrorPayload) => void;
}

export interface ClientEvents {
  "create-room": (payload: CreateRoomPayload) => void;
  "join-room": (payload: JoinRoomPayload) => void;
  "make-move": (payload: MakeMovePayload) => void;
  "reconnect": (payload: ReconnectPayload) => void;
}

export interface CreateRoomPayload {
  playerId: string;
}

export interface JoinRoomPayload {
  roomId: string;
  playerId: string;
}

export interface MakeMovePayload {
  roomId: string;
  playerId: string;
  row: number;
  col: number;
}

export interface ReconnectPayload {
  playerId: string;
}

export interface RoomCreatedPayload {
  roomId: string;
  player: PlayerSummary;
}

export interface PlayerJoinedPayload {
  playerId: string;
  symbol: "X" | "O";
  players: PlayerSummary[];
}

export interface GameStartedPayload {
  board: (string | null)[][];
  currentPlayer: PlayerSummary;
  turnDeadline: number;
  playerX: PlayerSummary;
  playerO: PlayerSummary;
}

export interface MoveResultPayload {
  success: boolean;
  error?: string;
  move?: MoveSummary;
  board?: (string | null)[][];
  currentPlayer?: PlayerSummary | null;
  turnDeadline?: number | null;
  winner?: PlayerSummary | null;
  winningCells?: [number, number][] | null;
  isDraw?: boolean;
}

export interface PlayerDisconnectedPayload {
  playerId: string;
}

export interface PlayerReconnectedPayload {
  playerId: string;
}

export interface RoomStatePayload {
  roomId: string;
  status: RoomStatus;
  players: PlayerSummary[];
  board: (string | null)[][];
  currentPlayer: PlayerSummary | null;
  turnDeadline: number | null;
  winner: PlayerSummary | null;
  winningCells: [number, number][] | null;
  isDraw: boolean;
  moves: MoveSummary[];
}

export interface ErrorPayload {
  message: string;
}

export interface PlayerSummary {
  playerId: string;
  symbol: "X" | "O";
  connected: boolean;
}

export interface MoveSummary {
  playerId: string;
  symbol: "X" | "O";
  row: number;
  col: number;
  timestamp: string;
}
