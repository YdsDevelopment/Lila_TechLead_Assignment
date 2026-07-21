import { Socket, Server as SocketIOServer } from "socket.io";
import { GameManager } from "../../game/GameManager";
import { clearTurnTimeout, scheduleTurnTimeout } from "../socket";
import { logger } from "../../utils/Logger";

export function handlePlayAgain(
  socket: Socket,
  io: SocketIOServer,
  gameManager: GameManager
): void {
  socket.on("play-again", (data: { roomId: string; playerId: string }) => {
    try {
      const room = gameManager.playAgain(data.roomId, data.playerId);
      const game = room.game;
      const currentPlayer = game.getCurrentPlayer();
      const turnDeadline = game.getTurnDeadline();

      clearTurnTimeout(data.roomId);

      io.to(data.roomId).emit("game-started", {
        board: game.getBoard(),
        currentPlayer: {
          playerId: currentPlayer!.playerId,
          symbol: currentPlayer!.symbol,
          connected: currentPlayer!.connected,
        },
        turnDeadline: turnDeadline!,
        playerX: {
          playerId: room.players[0].playerId,
          symbol: room.players[0].symbol,
          connected: room.players[0].connected,
        },
        playerO: {
          playerId: room.players[1].playerId,
          symbol: room.players[1].symbol,
          connected: room.players[1].connected,
        },
      });

      if (game.isTurnTimeoutEnabled() && turnDeadline) {
        const delay = turnDeadline - Date.now();
        if (delay > 0) {
          scheduleTurnTimeout(data.roomId, delay);
        }
      }

      logger.info(`Game restarted in room ${data.roomId} by ${data.playerId}`);
    } catch (err) {
      const error = err as Error;
      socket.emit("error", { message: error.message });
    }
  });
}
