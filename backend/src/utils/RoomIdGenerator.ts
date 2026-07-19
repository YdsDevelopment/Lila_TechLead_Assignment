import { v4 as uuidv4 } from "uuid";

export class RoomIdGenerator {
  static generate(): string {
    return uuidv4();
  }
}
