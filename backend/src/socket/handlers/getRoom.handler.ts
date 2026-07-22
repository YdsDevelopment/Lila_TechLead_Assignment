import { Socket } from "socket.io";
import { GameManager } from "../../game/GameManager";
import { logger } from "../../utils/Logger";

export function handleGetRoom(socket: Socket, gameManager: GameManager): void {
  socket.on("get-room", (data: { playerId: string; roomId: string }) => {
    try {
      const room = gameManager.getRoom(data.roomId);
      if (!room) {
        socket.emit("error", { message: `Room not found: ${data.roomId}` });
        return;
      }

      const game = room.game;
      const currentPlayer = game.getCurrentPlayer();

      socket.emit("room-details", {
        roomId: room.roomId,
        status: room.status,
        players: room.players.map((p) => ({
          playerId: p.playerId,
          symbol: p.symbol,
          connected: p.connected,
        })),
        game: {
          board: game.getBoard(),
          currentPlayer: currentPlayer
            ? {
                playerId: currentPlayer.playerId,
                symbol: currentPlayer.symbol,
                connected: currentPlayer.connected,
              }
            : null,
          turnDeadline: game.getTurnDeadline(),
          winner: game.getWinner()
            ? {
                playerId: game.getWinner()!.playerId,
                symbol: game.getWinner()!.symbol,
              }
            : null,
          winningCells: game.getWinningCells(),
          isDraw: game.isDrawGame(),
          moves: game.getMoves().map((m) => ({
            playerId: m.playerId,
            row: m.row,
            col: m.col,
            timestamp: m.timestamp.toISOString(),
          })),
          startedAt: game.getStartedAt()?.toISOString() ?? null,
          completedAt: game.getCompletedAt()?.toISOString() ?? null,
        },
        createdAt: room.createdAt.toISOString(),
        updatedAt: room.updatedAt.toISOString(),
      });

      logger.info(`Sent room details for ${data.roomId} to ${data.playerId || socket.id}`);
    } catch (err) {
      const error = err as Error;
      socket.emit("error", { message: error.message });
    }
  });
}
