import { Player } from "../models/Player";
import { Move } from "../models/Move";
import { WinnerChecker } from "./WinnerChecker";
import {
  GameNotStartedError,
  GameAlreadyOverError,
  NotYourTurnError,
  CellOccupiedError,
  MoveTimeoutError,
  InvalidMoveError,
} from "./GameError";

export interface MoveResult {
  success: boolean;
  error?: string;
  move?: Move;
  board: (string | null)[][];
  winner: Player | null;
  winningCells: [number, number][] | null;
  isDraw: boolean;
  currentPlayer: Player | null;
  turnDeadline: number | null;
}

export class GameEngine {
  private board: (string | null)[][];
  private moves: Move[];
  private currentPlayerIndex: number;
  private player1: Player | null;
  private player2: Player | null;
  private winner: Player | null;
  private winningCells: [number, number][] | null;
  private isDraw: boolean;
  private startedAt: Date | null;
  private completedAt: Date | null;
  private turnDeadline: number | null;
  private turnTimeoutMs: number;
  private winnerChecker: WinnerChecker;

  constructor() {
    this.board = [
      [null, null, null],
      [null, null, null],
      [null, null, null],
    ];
    this.moves = [];
    this.currentPlayerIndex = 0;
    this.player1 = null;
    this.player2 = null;
    this.winner = null;
    this.winningCells = null;
    this.isDraw = false;
    this.startedAt = null;
    this.completedAt = null;
    this.turnDeadline = null;
    this.turnTimeoutMs = 30000;
    this.winnerChecker = new WinnerChecker();
  }

  initialize(player1: Player, player2: Player, turnTimeoutMs: number): void {
    this.board = [
      [null, null, null],
      [null, null, null],
      [null, null, null],
    ];
    this.moves = [];
    this.currentPlayerIndex = 0;
    this.player1 = player1;
    this.player2 = player2;
    this.winner = null;
    this.winningCells = null;
    this.isDraw = false;
    this.startedAt = new Date();
    this.completedAt = null;
    this.turnTimeoutMs = turnTimeoutMs;
    this.turnDeadline = Date.now() + turnTimeoutMs;
  }

  makeMove(playerId: string, row: number, col: number): MoveResult {
    if (!this.startedAt) {
      throw new GameNotStartedError();
    }

    if (this.isDraw || this.winner) {
      throw new GameAlreadyOverError();
    }

    if (row < 0 || row > 2 || col < 0 || col > 2) {
      throw new InvalidMoveError(`Position (${row}, ${col}) is out of bounds`);
    }

    if (this.board[row][col] !== null) {
      throw new CellOccupiedError(row, col);
    }

    const currentPlayer = this.getCurrentPlayer();
    if (!currentPlayer) {
      throw new GameNotStartedError();
    }

    if (currentPlayer.playerId !== playerId) {
      throw new NotYourTurnError();
    }

    if (this.turnDeadline && Date.now() > this.turnDeadline) {
      throw new MoveTimeoutError();
    }

    const symbol = currentPlayer.symbol;
    this.board[row][col] = symbol;

    const move: Move = {
      playerId,
      row,
      col,
      timestamp: new Date(),
    };
    this.moves.push(move);

    const result = this.winnerChecker.check(this.board);

    if (result.winner) {
      this.winner = currentPlayer;
      this.winningCells = result.winningCells;
      this.completedAt = new Date();
      this.turnDeadline = null;

      return {
        success: true,
        move,
        board: this.cloneBoard(),
        winner: this.winner,
        winningCells: this.winningCells,
        isDraw: false,
        currentPlayer: null,
        turnDeadline: null,
      };
    }

    if (result.isDraw) {
      this.isDraw = true;
      this.completedAt = new Date();
      this.turnDeadline = null;

      return {
        success: true,
        move,
        board: this.cloneBoard(),
        winner: null,
        winningCells: null,
        isDraw: true,
        currentPlayer: null,
        turnDeadline: null,
      };
    }

    this.currentPlayerIndex = this.currentPlayerIndex === 0 ? 1 : 0;
    this.turnDeadline = Date.now() + this.turnTimeoutMs;

    return {
      success: true,
      move,
      board: this.cloneBoard(),
      winner: null,
      winningCells: null,
      isDraw: false,
      currentPlayer: this.getCurrentPlayer(),
      turnDeadline: this.turnDeadline,
    };
  }

  getBoard(): (string | null)[][] {
    return this.cloneBoard();
  }

  getCurrentPlayer(): Player | null {
    if (!this.player1 || !this.player2 || !this.startedAt) {
      return null;
    }
    return this.currentPlayerIndex === 0 ? this.player1 : this.player2;
  }

  getTurnDeadline(): number | null {
    return this.turnDeadline;
  }

  isGameOver(): boolean {
    return this.winner !== null || this.isDraw;
  }

  getWinner(): Player | null {
    return this.winner;
  }

  getWinningCells(): [number, number][] | null {
    return this.winningCells;
  }

  isDrawGame(): boolean {
    return this.isDraw;
  }

  getMoves(): Move[] {
    return [...this.moves];
  }

  getPlayer1(): Player | null {
    return this.player1;
  }

  getPlayer2(): Player | null {
    return this.player2;
  }

  getStartedAt(): Date | null {
    return this.startedAt;
  }

  getCompletedAt(): Date | null {
    return this.completedAt;
  }

  private cloneBoard(): (string | null)[][] {
    return this.board.map((row) => [...row]);
  }
}
