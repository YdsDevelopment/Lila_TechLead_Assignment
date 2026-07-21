import { defineConfig } from "vite";
import fs from "fs";
import path from "path";

export default defineConfig({
  server: {
    port: 5173,
  },
  plugins: [
    {
      name: "save-log",
      configureServer(server) {
        server.middlewares.use("/save-log", async (req, res) => {
          if (req.method !== "POST") {
            res.statusCode = 405;
            res.end(JSON.stringify({ error: "POST only" }));
            return;
          }
          let body = "";
          req.on("data", (c) => (body += c));
          req.on("end", () => {
            try {
              const { filename, content } = JSON.parse(body);
              const logsDir = path.resolve(process.cwd(), "eventlogs");
              if (!fs.existsSync(logsDir)) {
                fs.mkdirSync(logsDir, { recursive: true });
              }
              const filePath = path.join(logsDir, filename);
              fs.writeFileSync(filePath, content, "utf-8");
              res.statusCode = 200;
              res.end(JSON.stringify({ ok: true, path: filePath }));
            } catch (e) {
              res.statusCode = 500;
              res.end(JSON.stringify({ error: e.message }));
            }
          });
        });
      },
    },
  ],
});
