import { Socket } from "socket.io";
import { logger } from "../../utils/Logger";

export function handleGetHealth(socket: Socket): void {
  socket.on("get-health", (_data: { playerId: string }) => {
    socket.emit("health-status", {
      status: "ok",
      server: "running",
    });
    logger.info(`Health check sent to ${_data.playerId || socket.id}`);
  });
}
