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

let io: SocketIOServer;
let gameManager: GameManager;

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
  });

  logger.info("Socket.IO initialized");
  return io;
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
