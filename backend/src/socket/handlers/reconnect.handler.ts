import { Socket, Server as SocketIOServer } from "socket.io";
import { GameManager } from "../../game/GameManager";
import { logger } from "../../utils/Logger";

export function handleReconnect(
  socket: Socket,
  io: SocketIOServer,
  gameManager: GameManager
): void {
  socket.on("reconnect", (data: { playerId: string }) => {
    try {
      const result = gameManager.reconnectPlayer(socket.id, data.playerId);

      if (!result) {
        socket.emit("error", { message: "No active game found for reconnection" });
        return;
      }

      const { room } = result;
      socket.join(room.roomId);

      const game = room.game;
      const currentPlayer = game.getCurrentPlayer();
      const moves = game.getMoves().map((m) => ({
        playerId: m.playerId,
        row: m.row,
        col: m.col,
        timestamp: m.timestamp.toISOString(),
      }));

      socket.emit("room-state", {
        roomId: room.roomId,
        status: room.status,
        players: room.players.map((p) => ({
          playerId: p.playerId,
          symbol: p.symbol,
          connected: p.connected,
        })),
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
              connected: game.getWinner()!.connected,
            }
          : null,
        winningCells: game.getWinningCells(),
        isDraw: game.isDrawGame(),
        moves,
      });

      io.to(room.roomId).emit("player-reconnected", {
        playerId: data.playerId,
      });

      logger.info(`Player ${data.playerId} reconnected to room ${room.roomId}`);
    } catch (err) {
      const error = err as Error;
      socket.emit("error", { message: error.message });
    }
  });
}
