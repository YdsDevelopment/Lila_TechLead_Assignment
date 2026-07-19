import { Player } from "./Player";
import { RoomStatus } from "./RoomStatus";
import { GameEngine } from "../game/GameEngine";

export interface Room {
  roomId: string;
  status: RoomStatus;
  players: Player[];
  game: GameEngine;
  createdAt: Date;
  updatedAt: Date;
}
