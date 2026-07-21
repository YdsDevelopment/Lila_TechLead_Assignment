import dotenv from "dotenv";

dotenv.config();

export const env = {
  port: parseInt(process.env.PORT || "3000", 10),
  clientUrl: process.env.CLIENT_URL || "http://localhost:5173",
  turnTimeoutMs: parseInt(process.env.TURN_TIMEOUT || "30000", 10),
  turnTimeoutEnabled: process.env.TURN_TIMEOUT_ENABLED !== "false",
};
