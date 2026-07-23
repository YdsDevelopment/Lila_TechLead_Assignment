# How to Set Up & Host the Backend Server

## Prerequisites

| Requirement | Version | Windows | macOS |
|---|---|---|---|
| **Node.js** | 20+ | [nodejs.org](https://nodejs.org) or `winget install OpenJS.NodeJS.LTS` | `brew install node@20` |
| **npm** | 10+ (ships with Node) | Included with Node.js | Included with Node.js |
| **Git** | any | [git-scm.com](https://git-scm.com) or `winget install Git.Git` | `brew install git` or Xcode CLI tools |
| **Docker** (optional) | latest | [docker.com](https://www.docker.com/products/docker-desktop/) | `brew install --cask docker` |

---

## macOS

### 1. Install Node.js (if not installed)

```bash
# Using Homebrew (recommended)
brew install node@20

# Or using nvm (version manager)
curl -o- https://raw.githubusercontent.com/nvm-sh/nvm/v0.40.1/install.sh | bash
nvm install 20
```

### 2. Clone & Enter the Project

```bash
cd /path/to/LilaGames/Problem1/backend
```

### 3. One-Command Dependency Install

```bash
npm install
```

This installs all runtime deps (`cors`, `dotenv`, `express`, `socket.io`, `uuid`) and dev deps (`typescript`, `ts-node-dev`, `eslint`, `prettier`, `@types/*`).

### 4. Configure Environment

```bash
cp .env.example .env
# Edit .env if needed (defaults work for local dev)
```

### 5. Run the Server

```bash
# Development (with hot reload)
npm run dev

# OR production build
npm run build && npm start
```

Server starts at `http://localhost:5000`.

---

## Windows

### 1. Install Node.js (if not installed)

```powershell
# Using winget (recommended)
winget install OpenJS.NodeJS.LTS

# Or download from https://nodejs.org (v20 LTS)
```

### 2. Clone & Enter the Project

```powershell
cd \path\to\LilaGames\Problem1\backend
```

### 3. One-Command Dependency Install

```powershell
npm install
```

This installs all runtime and dev dependencies (same as macOS).

### 4. Configure Environment

```powershell
copy .env.example .env
# Edit .env if needed (defaults work for local dev)
```

### 5. Run the Server

```powershell
# Development (with hot reload)
npm run dev

# OR production build
npm run build && npm start
```

Server starts at `http://localhost:5000`.

---

## Single-Command Install (all dependencies)

Both Windows and macOS:

```bash
npm install
```

That's it — a single command. `npm install` reads `package.json` and installs everything:
- **Runtime**: cors, dotenv, express, socket.io, uuid
- **Dev**: typescript, ts-node-dev, eslint, prettier, @types/cors, @types/express, @types/node, @types/uuid

---

## Verify It Works

```bash
curl http://localhost:5000/health
# → {"status":"ok","server":"running"}
```

---

## Using Docker (alternative to local setup)

### macOS & Windows

```bash
# From the backend/ directory
docker compose up --build
```

This builds a production image (multi-stage: builder + runner) and starts on port 5000. No Node.js installation needed on the host — only Docker Desktop.

### Docker Commands Reference

| Command | Description |
|---|---|
| `docker compose up --build` | Build & start |
| `docker compose up -d` | Start in background |
| `docker compose down` | Stop & remove container |
| `docker compose logs -f` | Follow logs |

---

## Available npm Scripts

| Command | Description |
|---|---|
| `npm run dev` | Start with hot reload (ts-node-dev) |
| `npm run build` | Compile TypeScript → `dist/` |
| `npm start` | Run compiled JS from `dist/` |
| `npm run lint` | ESLint check all `src/` |
| `npm run format` | Prettier format all `src/` |

---

## Environment Variables

| Variable | Default | Description |
|---|---|---|
| `PORT` | `5000` | HTTP server port |
| `CLIENT_URL` | `http://localhost:5173` | CORS allowed origin |
| `TURN_TIMEOUT` | `30000` | Turn timeout in ms |
| `TURN_TIMEOUT_ENABLED` | `true` | Enable/disable turn timer |
