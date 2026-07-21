import { Server as HttpServer } from "http";
import { Server as SocketIOServer } from "socket.io";
import { env } from "../config/env";
import { logger } from "../utils/Logger";
import { GameManager } from "../game/GameManager";
import { handleCreateRoom } from "./handlers/createRoom.handler";
import { handleJoinRoom } from "./handlers/joinRoom.handler";
import { handleMakeMove } from "./handlers/makeMove.handler";
import { handleReconnect } from "./handlers/reconnect.handler";
import { handleDisconnect } from "./handlers/disconnect.handler";
import { handlePlayAgain } from "./handlers/playAgain.handler";
import { handleLeaveRoom } from "./handlers/leaveRoom.handler";

let io: SocketIOServer;
let gameManager: GameManager;
const turnTimeouts = new Map<string, NodeJS.Timeout>();

export function initializeSocket(httpServer: HttpServer): SocketIOServer {
  gameManager = new GameManager();

  io = new SocketIOServer(httpServer, {
    cors: {
      origin: env.clientUrl,
      methods: ["GET", "POST"],
    },
  });

  io.on("connection", (socket) => {
    logger.socketConnection(socket.id);

    handleCreateRoom(socket, gameManager);
    handleJoinRoom(socket, io, gameManager);
    handleMakeMove(socket, io, gameManager);
    handleReconnect(socket, io, gameManager);
    handleDisconnect(socket, io, gameManager);
    handlePlayAgain(socket, io, gameManager);
    handleLeaveRoom(socket, io, gameManager);
  });

  startTimerBroadcast();

  logger.info("Socket.IO initialized");
  return io;
}

function startTimerBroadcast(): void {
  setInterval(() => {
    const gm = getGameManager();
    const rooms = gm.getActiveRooms();
    const ioInstance = getIO();
    for (const room of rooms) {
      const game = room.game;
      const turnDeadline = game.getTurnDeadline();
      const currentPlayer = game.getCurrentPlayer();
      if (!turnDeadline || !currentPlayer || game.isGameOver()) continue;
      const remainingMs = Math.max(0, turnDeadline - Date.now());
      ioInstance.to(room.roomId).emit("turn-timer", {
        roomId: room.roomId,
        remainingMs,
        turnDeadline,
        currentPlayer: {
          playerId: currentPlayer.playerId,
          symbol: currentPlayer.symbol,
          connected: currentPlayer.connected,
        },
      });
    }
  }, 1000);
}

export function scheduleTurnTimeout(roomId: string, delayMs: number): void {
  clearTurnTimeout(roomId);
  const timeout = setTimeout(() => {
    const gm = getGameManager();
    const result = gm.checkTimeout(roomId);
    if (result) {
      const ioInstance = getIO();
      const payload: Record<string, unknown> = {
        success: true,
        board: result.board,
        winner: result.winner ? {
          playerId: result.winner.playerId,
          symbol: result.winner.symbol,
          connected: result.winner.connected,
        } : null,
        winningCells: null,
        isDraw: false,
        currentPlayer: null,
        turnDeadline: null,
        timeoutWin: true,
      };
      ioInstance.to(roomId).emit("move-result", payload);
      logger.info(`Game ${roomId} finished: opponent won by timeout`);
    }
    turnTimeouts.delete(roomId);
  }, delayMs);
  turnTimeouts.set(roomId, timeout);
}

export function clearTurnTimeout(roomId: string): void {
  const existing = turnTimeouts.get(roomId);
  if (existing) {
    clearTimeout(existing);
    turnTimeouts.delete(roomId);
  }
}

export function getIO(): SocketIOServer {
  if (!io) {
    throw new Error("Socket.IO not initialized");
  }
  return io;
}

export function getGameManager(): GameManager {
  if (!gameManager) {
    throw new Error("GameManager not initialized");
  }
  return gameManager;
}
