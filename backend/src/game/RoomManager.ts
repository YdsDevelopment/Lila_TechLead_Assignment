import { Room } from "../models/Room";
import { Player, CreatePlayerParams } from "../models/Player";
import { RoomStatus } from "../models/RoomStatus";
import { GameEngine } from "./GameEngine";
import { RoomIdGenerator } from "../utils/RoomIdGenerator";
import { logger } from "../utils/Logger";
import {
  RoomNotFoundError,
  RoomFullError,
  PlayerAlreadyInRoomError,
  DuplicatePlayerError,
  DuplicateSocketError,
  PlayerNotInRoomError,
} from "./RoomError";

export class RoomManager {
  private rooms: Map<string, Room>;
  private playerToRoom: Map<string, string>;
  private socketToRoom: Map<string, string>;

  constructor() {
    this.rooms = new Map();
    this.playerToRoom = new Map();
    this.socketToRoom = new Map();
  }

  createRoom(hostPlayer: CreatePlayerParams): Room {
    this.validatePlayerNotInAnyRoom(hostPlayer.playerId);
    this.validateSocketNotInAnyRoom(hostPlayer.socketId);

    const player = this.createPlayer(hostPlayer, "X");
    const room: Room = {
      roomId: RoomIdGenerator.generate(),
      status: RoomStatus.OPEN,
      players: [player],
      game: new GameEngine(),
      createdAt: new Date(),
      updatedAt: new Date(),
    };

    this.rooms.set(room.roomId, room);
    this.indexPlayer(player.playerId, room.roomId);
    this.indexSocket(player.socketId, room.roomId);

    logger.info(`Room created: ${room.roomId} by player ${player.playerId}`);
    return room;
  }

  joinRoom(roomId: string, player: CreatePlayerParams): Room {
    const room = this.getRoomOrThrow(roomId);

    this.validatePlayerNotInAnyRoom(player.playerId);
    this.validateSocketNotInAnyRoom(player.socketId);
    this.validateRoomNotFull(room);
    this.validatePlayerNotDuplicateInRoom(room, player.playerId);
    this.validateSocketNotDuplicateInRoom(room, player.socketId);

    const newPlayer = this.createPlayer(player, "O");
    room.players.push(newPlayer);
    room.updatedAt = new Date();

    if (room.players.length === 2) {
      room.status = RoomStatus.ACTIVE;
      logger.info(`Room activated: ${room.roomId}`);
    }

    this.indexPlayer(player.playerId, room.roomId);
    this.indexSocket(player.socketId, room.roomId);

    logger.info(`Player ${player.playerId} joined room ${room.roomId}`);
    return room;
  }

  leaveRoom(playerId: string): void {
    const room = this.getRoomByPlayerOrThrow(playerId);
    const playerIndex = room.players.findIndex((p) => p.playerId === playerId);

    if (playerIndex === -1) {
      return;
    }

    const player = room.players[playerIndex];
    room.players.splice(playerIndex, 1);
    room.updatedAt = new Date();

    this.deindexPlayer(player.playerId);
    this.deindexSocket(player.socketId);

    if (room.players.length === 0) {
      this.deleteRoom(room.roomId);
      logger.info(`Room deleted (empty): ${room.roomId}`);
      return;
    }

    if (room.status === RoomStatus.ACTIVE) {
      room.status = RoomStatus.OPEN;
      logger.info(`Room ${room.roomId} reverted to OPEN`);
    }

    logger.info(`Player ${playerId} left room ${room.roomId}`);
  }

  getRoom(roomId: string): Room | undefined {
    return this.rooms.get(roomId);
  }

  getRoomByPlayer(playerId: string): Room | undefined {
    const roomId = this.playerToRoom.get(playerId);
    if (!roomId) {
      return undefined;
    }
    return this.rooms.get(roomId);
  }

  getRoomBySocket(socketId: string): Room | undefined {
    const roomId = this.socketToRoom.get(socketId);
    if (!roomId) {
      return undefined;
    }
    return this.rooms.get(roomId);
  }

  hasRoom(roomId: string): boolean {
    return this.rooms.has(roomId);
  }

  deleteRoom(roomId: string): void {
    const room = this.rooms.get(roomId);
    if (!room) {
      return;
    }

    for (const player of room.players) {
      this.deindexPlayer(player.playerId);
      this.deindexSocket(player.socketId);
    }

    this.rooms.delete(roomId);
    logger.info(`Room deleted: ${roomId}`);
  }

  getOpenRooms(): Room[] {
    return Array.from(this.rooms.values()).filter(
      (room) => room.status === RoomStatus.OPEN
    );
  }

  getActiveRooms(): Room[] {
    return Array.from(this.rooms.values()).filter(
      (room) => room.status === RoomStatus.ACTIVE
    );
  }

  getRoomCount(): number {
    return this.rooms.size;
  }

  getOpenRoomCount(): number {
    return this.getOpenRooms().length;
  }

  getActiveRoomCount(): number {
    return this.getActiveRooms().length;
  }

  updatePlayerSocket(playerId: string, newSocketId: string): Player {
    const room = this.getRoomByPlayerOrThrow(playerId);
    const player = room.players.find((p) => p.playerId === playerId);
    if (!player) {
      throw new PlayerNotInRoomError(playerId);
    }

    this.deindexSocket(player.socketId);
    player.socketId = newSocketId;
    this.indexSocket(newSocketId, room.roomId);
    room.updatedAt = new Date();

    return player;
  }

  markPlayerDisconnected(playerId: string): void {
    const room = this.getRoomByPlayerOrThrow(playerId);
    const player = room.players.find((p) => p.playerId === playerId);
    if (player) {
      player.connected = false;
      room.updatedAt = new Date();
    }
  }

  markPlayerConnected(playerId: string): void {
    const room = this.getRoomByPlayerOrThrow(playerId);
    const player = room.players.find((p) => p.playerId === playerId);
    if (player) {
      player.connected = true;
      room.updatedAt = new Date();
    }
  }

  private createPlayer(params: CreatePlayerParams, symbol: "X" | "O"): Player {
    return {
      playerId: params.playerId,
      socketId: params.socketId,
      symbol,
      connected: true,
      joinedAt: new Date(),
    };
  }

  private indexPlayer(playerId: string, roomId: string): void {
    this.playerToRoom.set(playerId, roomId);
  }

  private indexSocket(socketId: string, roomId: string): void {
    this.socketToRoom.set(socketId, roomId);
  }

  private deindexPlayer(playerId: string): void {
    this.playerToRoom.delete(playerId);
  }

  private deindexSocket(socketId: string): void {
    this.socketToRoom.delete(socketId);
  }

  getRoomOrThrow(roomId: string): Room {
    const room = this.rooms.get(roomId);
    if (!room) {
      throw new RoomNotFoundError(roomId);
    }
    return room;
  }

  private getRoomByPlayerOrThrow(playerId: string): Room {
    const room = this.getRoomByPlayer(playerId);
    if (!room) {
      throw new PlayerNotInRoomError(playerId);
    }
    return room;
  }

  private validatePlayerNotInAnyRoom(playerId: string): void {
    if (this.playerToRoom.has(playerId)) {
      throw new PlayerAlreadyInRoomError(playerId);
    }
  }

  private validateSocketNotInAnyRoom(socketId: string): void {
    if (this.socketToRoom.has(socketId)) {
      throw new DuplicateSocketError(socketId);
    }
  }

  private validateRoomNotFull(room: Room): void {
    if (room.players.length >= 2) {
      throw new RoomFullError(room.roomId);
    }
  }

  private validatePlayerNotDuplicateInRoom(
    room: Room,
    playerId: string
  ): void {
    if (room.players.some((p) => p.playerId === playerId)) {
      throw new DuplicatePlayerError(playerId);
    }
  }

  private validateSocketNotDuplicateInRoom(
    room: Room,
    socketId: string
  ): void {
    if (room.players.some((p) => p.socketId === socketId)) {
      throw new DuplicateSocketError(socketId);
    }
  }
}
