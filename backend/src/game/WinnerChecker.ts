export interface WinnerCheckResult {
  winner: "X" | "O" | null;
  winningCells: [number, number][] | null;
  isDraw: boolean;
}

const WINNING_LINES: [number, number][][] = [
  [[0, 0], [0, 1], [0, 2]],
  [[1, 0], [1, 1], [1, 2]],
  [[2, 0], [2, 1], [2, 2]],
  [[0, 0], [1, 0], [2, 0]],
  [[0, 1], [1, 1], [2, 1]],
  [[0, 2], [1, 2], [2, 2]],
  [[0, 0], [1, 1], [2, 2]],
  [[0, 2], [1, 1], [2, 0]],
];

export class WinnerChecker {
  check(board: (string | null)[][]): WinnerCheckResult {
    for (const line of WINNING_LINES) {
      const [a, b, c] = line;
      const valA = board[a[0]][a[1]];
      const valB = board[b[0]][b[1]];
      const valC = board[c[0]][c[1]];

      if (valA && valA === valB && valB === valC) {
        return {
          winner: valA as "X" | "O",
          winningCells: line,
          isDraw: false,
        };
      }
    }

    const isDraw = board.every((row) => row.every((cell) => cell !== null));

    return {
      winner: null,
      winningCells: null,
      isDraw,
    };
  }
}
