export class RoomError extends Error {
  constructor(message: string) {
    super(message);
    this.name = "RoomError";
  }
}

export class RoomNotFoundError extends RoomError {
  constructor(roomId: string) {
    super(`Room not found: ${roomId}`);
    this.name = "RoomNotFoundError";
  }
}

export class RoomFullError extends RoomError {
  constructor(roomId: string) {
    super(`Room is full: ${roomId}`);
    this.name = "RoomFullError";
  }
}

export class PlayerAlreadyInRoomError extends RoomError {
  constructor(playerId: string) {
    super(`Player already in a room: ${playerId}`);
    this.name = "PlayerAlreadyInRoomError";
  }
}

export class DuplicatePlayerError extends RoomError {
  constructor(playerId: string) {
    super(`Duplicate player in room: ${playerId}`);
    this.name = "DuplicatePlayerError";
  }
}

export class DuplicateSocketError extends RoomError {
  constructor(socketId: string) {
    super(`Socket already in a room: ${socketId}`);
    this.name = "DuplicateSocketError";
  }
}

export class InvalidRoomStateError extends RoomError {
  constructor(roomId: string, expected: string, actual: string) {
    super(`Invalid room state for ${roomId}: expected ${expected}, got ${actual}`);
    this.name = "InvalidRoomStateError";
  }
}

export class PlayerNotInRoomError extends RoomError {
  constructor(playerId: string) {
    super(`Player not found in any room: ${playerId}`);
    this.name = "PlayerNotInRoomError";
  }
}
