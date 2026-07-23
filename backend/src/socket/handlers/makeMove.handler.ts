import { Socket, Server as SocketIOServer } from "socket.io";
import { GameManager } from "../../game/GameManager";
import { scheduleTurnTimeout, clearTurnTimeout } from "../socket";
import { logger } from "../../utils/Logger";

export function handleMakeMove(
  socket: Socket,
  io: SocketIOServer,
  gameManager: GameManager
): void {
  socket.on(
    "make-move",
    (data: { roomId: string; playerId: string; row: number; col: number }) => {
      try {
        const result = gameManager.makeMove(data.roomId, data.playerId, data.row, data.col);

        const payload: Record<string, unknown> = {
          success: result.success,
        };

        if (result.error) {
          payload.error = result.error;
          payload.move = {
            playerId: data.playerId,
            row: data.row,
            col: data.col,
            timestamp: "",
          };
        }
        if (result.move) {
          payload.move = {
            playerId: result.move.playerId,
            row: result.move.row,
            col: result.move.col,
            timestamp: result.move.timestamp.toISOString(),
          };
        }
        if (result.board) {
          payload.board = result.board;
        }
        if (result.currentPlayer) {
          payload.currentPlayer = {
            playerId: result.currentPlayer.playerId,
            symbol: result.currentPlayer.symbol,
            connected: result.currentPlayer.connected,
          };
        } else {
          payload.currentPlayer = null;
        }
        if (result.turnDeadline !== undefined) {
          payload.turnDeadline = result.turnDeadline;
        }
        if (result.winner) {
          payload.winner = {
            playerId: result.winner.playerId,
            symbol: result.winner.symbol,
            connected: result.winner.connected,
          };
        } else {
          payload.winner = null;
        }
        if (result.winningCells) {
          payload.winningCells = result.winningCells;
        }
        payload.isDraw = result.isDraw;

        io.to(data.roomId).emit("move-result", payload);

        if (result.winner || result.isDraw) {
          clearTurnTimeout(data.roomId);
          logger.info(
            `Game ${data.roomId} finished: ${result.winner ? `winner ${result.winner.playerId}` : "draw"}`
          );
        } else if (result.success && result.currentPlayer) {
          const room = gameManager.getRoom(data.roomId);
          const delay = result.turnDeadline ? result.turnDeadline - Date.now() : 0;
          if (delay > 0 && room?.game.isTurnTimeoutEnabled()) {
            scheduleTurnTimeout(data.roomId, delay);
          }
        }
      } catch (err) {
        const error = err as Error;
        io.to(data.roomId).emit("move-result", {
          success: false,
          error: error.message,
        });
      }
    }
  );
}
