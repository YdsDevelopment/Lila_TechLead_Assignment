import { Router } from "express";
import { RoomController } from "../controllers/RoomController";

const router = Router();
const roomController = new RoomController();

router.get("/rooms", (req, res) => roomController.getAll(req, res));
router.get("/rooms/:id", (req, res) => roomController.getById(req, res));
router.post("/rooms", (req, res) => roomController.create(req, res));

export default router;
