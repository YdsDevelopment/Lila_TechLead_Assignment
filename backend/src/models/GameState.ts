import { Player } from "./Player";
import { Move } from "./Move";

export interface GameState {
  board: (string | null)[][];
  currentPlayer: Player;
  moves: Move[];
  winner: Player | null;
  isDraw: boolean;
  startedAt: Date;
  completedAt: Date | null;
}
