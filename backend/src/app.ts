import express from "express";
import cors from "cors";
import healthRoutes from "./routes/health.routes";
import roomRoutes from "./routes/room.routes";
import { errorHandler } from "./middleware/errorHandler";
import { notFound } from "./middleware/notFound";
import { env } from "./config/env";

const app = express();

app.use(
  cors({
    origin: env.clientUrl,
  })
);
app.use(express.json());

app.use(healthRoutes);
app.use(roomRoutes);

app.use(notFound);
app.use(errorHandler);

export default app;
