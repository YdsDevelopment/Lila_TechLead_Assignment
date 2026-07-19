import { Socket } from "socket.io";
import { GameManager } from "../../game/GameManager";
import { logger } from "../../utils/Logger";

export function handleCreateRoom(socket: Socket, gameManager: GameManager): void {
  socket.on("create-room", (data: { playerId: string }) => {
    try {
      const room = gameManager.createRoom({
        playerId: data.playerId,
        socketId: socket.id,
      });

      socket.join(room.roomId);

      socket.emit("room-created", {
        roomId: room.roomId,
        player: {
          playerId: room.players[0].playerId,
          symbol: room.players[0].symbol,
          connected: room.players[0].connected,
        },
      });

      logger.info(`Room ${room.roomId} created by ${data.playerId}`);
    } catch (err) {
      const error = err as Error;
      socket.emit("error", { message: error.message });
    }
  });
}
