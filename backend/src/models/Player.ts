export interface Player {
  playerId: string;
  socketId: string;
  symbol: "X" | "O";
  connected: boolean;
  joinedAt: Date;
}

export interface CreatePlayerParams {
  playerId: string;
  socketId: string;
}
