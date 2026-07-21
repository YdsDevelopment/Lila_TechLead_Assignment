import { Socket, Server as SocketIOServer } from "socket.io";
import { GameManager } from "../../game/GameManager";
import { clearTurnTimeout } from "../socket";
import { logger } from "../../utils/Logger";

export function handleDisconnect(
  socket: Socket,
  io: SocketIOServer,
  gameManager: GameManager
): void {
  socket.on("disconnect", () => {
    logger.socketDisconnection(socket.id);

    const room = gameManager.getRoomBySocket(socket.id);
    if (!room) return;

    const player = room.players.find((p) => p.socketId === socket.id);
    if (!player) return;

    if (room.status === "FINISHED") {
      gameManager.leaveRoom(player.playerId);
      io.to(room.roomId).emit("player-left", {
        playerId: player.playerId,
        roomId: room.roomId,
        remainingPlayers: room.players.length,
        roomStatus: room.status,
      });
      clearTurnTimeout(room.roomId);
    } else {
      const result = gameManager.disconnectPlayer(socket.id);
      if (result) {
        io.to(result.roomId).emit("player-disconnected", {
          playerId: result.playerId,
        });
      }
    }
  });
}
