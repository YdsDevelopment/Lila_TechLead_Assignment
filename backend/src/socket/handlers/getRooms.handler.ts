import { Socket } from "socket.io";
import { GameManager } from "../../game/GameManager";
import { logger } from "../../utils/Logger";

export function handleGetRooms(socket: Socket, gameManager: GameManager): void {
  socket.on("get-rooms", (_data: { playerId: string }) => {
    try {
      const rooms = gameManager.getOpenRooms();
      const list = rooms.map((room) => ({
        roomId: room.roomId,
        status: room.status,
        playerCount: room.players.length,
        players: room.players.map((p) => ({
          playerId: p.playerId,
          symbol: p.symbol,
          connected: p.connected,
        })),
        createdAt: room.createdAt.toISOString(),
        updatedAt: room.updatedAt.toISOString(),
      }));

      socket.emit("rooms-list", { rooms: list });
      logger.info(`Sent rooms list to ${_data.playerId || socket.id}`);
    } catch (err) {
      const error = err as Error;
      socket.emit("error", { message: error.message });
    }
  });
}
