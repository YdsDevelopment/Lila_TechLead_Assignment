import { Socket, Server as SocketIOServer } from "socket.io";
import { GameManager } from "../../game/GameManager";
import { logger } from "../../utils/Logger";

export function handleJoinRoom(
  socket: Socket,
  io: SocketIOServer,
  gameManager: GameManager
): void {
  socket.on("join-room", (data: { roomId: string; playerId: string }) => {
    try {
      const { room, player } = gameManager.joinRoom(data.roomId, {
        playerId: data.playerId,
        socketId: socket.id,
      });

      socket.join(room.roomId);

      const playerSummary = {
        playerId: player.playerId,
        symbol: player.symbol,
        connected: player.connected,
      };

      socket.emit("room-joined", {
        roomId: room.roomId,
        player: playerSummary,
      });

      io.to(room.roomId).emit("player-joined", {
        playerId: data.playerId,
        symbol: player.symbol,
        players: room.players.map((p) => ({
          playerId: p.playerId,
          symbol: p.symbol,
          connected: p.connected,
        })),
      });

      if (room.players.length === 2) {
        const game = room.game;
        const currentPlayer = game.getCurrentPlayer();
        const turnDeadline = game.getTurnDeadline();

        io.to(room.roomId).emit("game-started", {
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

        logger.info(`Game started in room ${room.roomId}`);
      }
    } catch (err) {
      const error = err as Error;
      socket.emit("error", { message: error.message });
    }
  });
}
