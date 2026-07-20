import { Request, Response, NextFunction } from "express";
import { logger } from "../utils/Logger";

const errorStatusMap: Record<string, number> = {
  RoomNotFoundError: 404,
  RoomFullError: 409,
  PlayerAlreadyInRoomError: 409,
  PlayerNotInRoomError: 404,
};

export function errorHandler(
  err: Error,
  _req: Request,
  res: Response,
  _next: NextFunction
): void {
  const status = errorStatusMap[err.name] ?? 500;
  logger.error(`Error ${status}: ${err.message}`);
  res.status(status).json({
    error: err.message,
  });
}
