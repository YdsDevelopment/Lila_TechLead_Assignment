type LogLevel = "info" | "warn" | "error";

class Logger {
  private timestamp(): string {
    return new Date().toISOString();
  }

  private log(level: LogLevel, message: string): void {
    const entry = `[${this.timestamp()}] [${level.toUpperCase()}] ${message}`;
    switch (level) {
      case "error":
        console.error(entry);
        break;
      case "warn":
        console.warn(entry);
        break;
      default:
        console.log(entry);
    }
  }

  info(message: string): void {
    this.log("info", message);
  }

  warn(message: string): void {
    this.log("warn", message);
  }

  error(message: string): void {
    this.log("error", message);
  }

  serverStartup(port: number): void {
    this.info(`Server started on port ${port}`);
  }

  socketConnection(socketId: string): void {
    this.info(`Socket connected: ${socketId}`);
  }

  socketDisconnection(socketId: string): void {
    this.info(`Socket disconnected: ${socketId}`);
  }
}

export const logger = new Logger();
