import { Socket, Server as SocketIOServer } from "socket.io";
import { GameManager } from "../../game/GameManager";
import { clearTurnTimeout } from "../socket";
import { logger } from "../../utils/Logger";

export function handleLeaveRoom(
  socket: Socket,
  io: SocketIOServer,
  gameManager: GameManager
): void {
  socket.on("leave-room", (data: { roomId: string; playerId: string }) => {
    try {
      const result = gameManager.leaveRoom(data.playerId);
      if (!result) {
        socket.emit("error", { message: "You are not in a room" });
        return;
      }

      clearTurnTimeout(data.roomId);

      io.to(data.roomId).emit("player-left", {
        playerId: data.playerId,
        roomId: result.roomId,
        remainingPlayers: result.remainingPlayers,
        roomStatus: result.roomStatus,
      });

      socket.leave(data.roomId);

      logger.info(`Player ${data.playerId} left room ${data.roomId}`);
    } catch (err) {
      const error = err as Error;
      socket.emit("error", { message: error.message });
    }
  });
}
