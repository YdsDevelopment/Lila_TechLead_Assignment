import { RoomManager } from "./RoomManager";
import { MoveResult } from "./GameEngine";
import { Room } from "../models/Room";
import { Player } from "../models/Player";
import { RoomStatus } from "../models/RoomStatus";
import { CreatePlayerParams } from "../models/Player";
import { logger } from "../utils/Logger";
import { env } from "../config/env";

export class GameManager {
  private roomManager: RoomManager;
  private turnTimeoutMs: number;
  private turnTimeoutEnabled: boolean;

  constructor(turnTimeoutMs?: number, turnTimeoutEnabled?: boolean) {
    this.roomManager = new RoomManager();
    this.turnTimeoutMs = turnTimeoutMs ?? env.turnTimeoutMs;
    this.turnTimeoutEnabled = turnTimeoutEnabled ?? env.turnTimeoutEnabled;
  }

  createRoom(params: CreatePlayerParams): Room {
    const room = this.roomManager.createRoom(params);
    return room;
  }

  joinRoom(roomId: string, params: CreatePlayerParams): { room: Room; player: Player } {
    const room = this.roomManager.joinRoom(roomId, params);

    const player = room.players.find((p) => p.playerId === params.playerId);
    if (!player) {
      throw new Error("Player not found after joining");
    }

    if (room.players.length === 2 && room.status === RoomStatus.ACTIVE) {
      const game = room.game;
      game.initialize(room.players[0], room.players[1], this.turnTimeoutMs, this.turnTimeoutEnabled);
      logger.info(`Game started in room ${room.roomId}`);
    }

    return { room, player };
  }

  checkTimeout(roomId: string): MoveResult | null {
    const room = this.roomManager.getRoom(roomId);
    if (!room || room.status !== RoomStatus.ACTIVE) {
      return null;
    }

    const result = room.game.checkTimeout();
    if (result) {
      room.status = RoomStatus.FINISHED;
      room.updatedAt = new Date();
      logger.info(
        `Game over in room ${room.roomId}: opponent won by timeout`
      );
    }

    return result;
  }

  makeMove(roomId: string, playerId: string, row: number, col: number): MoveResult {
    const room = this.roomManager.getRoomOrThrow(roomId);

    if (room.status !== RoomStatus.ACTIVE) {
      return {
        success: false,
        error: "Game is not active",
        board: room.game.getBoard(),
        winner: null,
        winningCells: null,
        isDraw: false,
        currentPlayer: null,
        turnDeadline: null,
      };
    }

    try {
      const result = room.game.makeMove(playerId, row, col);

      if (result.winner || result.isDraw) {
        room.status = RoomStatus.FINISHED;
        room.updatedAt = new Date();
        logger.info(
          `Game over in room ${room.roomId}: ${result.winner ? `winner ${result.winner.playerId}` : "draw"}`
        );
      }

      return result;
    } catch (err) {
      const error = err as Error;
      return {
        success: false,
        error: error.message,
        board: room.game.getBoard(),
        winner: null,
        winningCells: null,
        isDraw: false,
        currentPlayer: room.game.getCurrentPlayer(),
        turnDeadline: room.game.getTurnDeadline(),
      };
    }
  }

  playAgain(roomId: string, playerId: string): Room {
    const room = this.roomManager.getRoomOrThrow(roomId);
    if (room.status !== RoomStatus.FINISHED) {
      throw new Error("Game is not finished");
    }
    if (!room.players.some((p) => p.playerId === playerId)) {
      throw new Error("Player not in this room");
    }
    this.roomManager.resetGame(roomId, this.turnTimeoutMs, this.turnTimeoutEnabled);
    logger.info(`Play again in room ${roomId} by ${playerId}`);
    return room;
  }

  leaveRoom(playerId: string): { roomId: string; remainingPlayers: number; roomStatus: RoomStatus } | null {
    const room = this.roomManager.getRoomByPlayer(playerId);
    if (!room) {
      return null;
    }
    const player = room.players.find((p) => p.playerId === playerId);
    if (player) {
      this.roomManager.leaveRoom(playerId);
      logger.info(`Player ${playerId} left room ${room.roomId}`);
      return {
        roomId: room.roomId,
        remainingPlayers: room.players.length,
        roomStatus: room.status,
      };
    }
    return null;
  }

  disconnectPlayer(socketId: string): { playerId: string; roomId: string } | null {
    const room = this.roomManager.getRoomBySocket(socketId);
    if (!room) {
      return null;
    }

    const player = room.players.find((p) => p.socketId === socketId);
    if (!player) {
      return null;
    }

    this.roomManager.markPlayerDisconnected(player.playerId);
    logger.info(`Player ${player.playerId} disconnected from room ${room.roomId}`);

    return { playerId: player.playerId, roomId: room.roomId };
  }

  reconnectPlayer(socketId: string, playerId: string): { room: Room; player: Player } | null {
    const room = this.roomManager.getRoomByPlayer(playerId);
    if (!room) {
      return null;
    }

    const player = this.roomManager.updatePlayerSocket(playerId, socketId);
    this.roomManager.markPlayerConnected(playerId);

    logger.info(`Player ${playerId} reconnected to room ${room.roomId}`);
    return { room, player };
  }

  getRoom(roomId: string): Room | undefined {
    return this.roomManager.getRoom(roomId);
  }

  getRoomByPlayer(playerId: string): Room | undefined {
    return this.roomManager.getRoomByPlayer(playerId);
  }

  getRoomBySocket(socketId: string): Room | undefined {
    return this.roomManager.getRoomBySocket(socketId);
  }

  getOpenRooms(): Room[] {
    return this.roomManager.getOpenRooms();
  }

  getActiveRooms(): Room[] {
    return this.roomManager.getActiveRooms();
  }

  getRoomManager(): RoomManager {
    return this.roomManager;
  }
}
