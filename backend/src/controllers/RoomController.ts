import { Request, Response } from "express";

export class RoomController {
  getAll(_req: Request, res: Response): void {
    res.status(501).json({ error: "Not implemented" });
  }

  getById(_req: Request, res: Response): void {
    res.status(501).json({ error: "Not implemented" });
  }

  create(_req: Request, res: Response): void {
    res.status(501).json({ error: "Not implemented" });
  }
}
