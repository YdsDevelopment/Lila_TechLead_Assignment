export class GameError extends Error {
  constructor(message: string) {
    super(message);
    this.name = "GameError";
  }
}

export class GameNotStartedError extends GameError {
  constructor() {
    super("Game has not started");
    this.name = "GameNotStartedError";
  }
}

export class GameAlreadyOverError extends GameError {
  constructor() {
    super("Game is already over");
    this.name = "GameAlreadyOverError";
  }
}

export class InvalidMoveError extends GameError {
  constructor(message: string) {
    super(message);
    this.name = "InvalidMoveError";
  }
}

export class NotYourTurnError extends GameError {
  constructor() {
    super("It is not your turn");
    this.name = "NotYourTurnError";
  }
}

export class CellOccupiedError extends GameError {
  constructor(row: number, col: number) {
    super(`Cell (${row}, ${col}) is already occupied`);
    this.name = "CellOccupiedError";
  }
}

export class MoveTimeoutError extends GameError {
  constructor() {
    super("Turn time has expired");
    this.name = "MoveTimeoutError";
  }
}

export class PlayerNotInGameError extends GameError {
  constructor(playerId: string) {
    super(`Player ${playerId} is not in this game`);
    this.name = "PlayerNotInGameError";
  }
}
