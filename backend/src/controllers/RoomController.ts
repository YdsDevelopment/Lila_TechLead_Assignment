import { Request, Response } from "express";
import { getGameManager } from "../socket/socket";
import { RoomNotFoundError } from "../game/RoomError";

export class RoomController {
  getAll(_req: Request, res: Response): void {
    const gameManager = getGameManager();
    const rooms = gameManager.getOpenRooms();

    const data = rooms.map((room) => ({
      roomId: room.roomId,
      status: room.status,
      playerCount: room.players.length,
      players: room.players.map((p) => ({
        playerId: p.playerId,
        symbol: p.symbol,
        connected: p.connected,
      })),
      createdAt: room.createdAt,
      updatedAt: room.updatedAt,
    }));

    res.json(data);
  }

  getById(req: Request, res: Response): void {
    const { id } = req.params;
    const gameManager = getGameManager();
    const room = gameManager.getRoom(id);

    if (!room) {
      throw new RoomNotFoundError(id);
    }

    const game = room.game;
    const currentPlayer = game.getCurrentPlayer();

    res.json({
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
          timestamp: m.timestamp,
        })),
        startedAt: game.getStartedAt(),
        completedAt: game.getCompletedAt(),
      },
      createdAt: room.createdAt,
      updatedAt: room.updatedAt,
    });
  }

  create(_req: Request, res: Response): void {
    res.status(501).json({ error: "Not implemented" });
  }
}
