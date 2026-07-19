import { Socket, Server as SocketIOServer } from "socket.io";
import { GameManager } from "../../game/GameManager";
import { logger } from "../../utils/Logger";

export function handleDisconnect(
  socket: Socket,
  io: SocketIOServer,
  gameManager: GameManager
): void {
  socket.on("disconnect", () => {
    logger.socketDisconnection(socket.id);

    const result = gameManager.disconnectPlayer(socket.id);
    if (result) {
      io.to(result.roomId).emit("player-disconnected", {
        playerId: result.playerId,
      });
    }
  });
}
