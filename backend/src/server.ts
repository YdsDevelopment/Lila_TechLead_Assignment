import http from "http";
import app from "./app";
import { env } from "./config/env";
import { logger } from "./utils/Logger";
import { initializeSocket } from "./socket/socket";

const server = http.createServer(app);

initializeSocket(server);

server.listen(env.port, () => {
  logger.serverStartup(env.port);
});
