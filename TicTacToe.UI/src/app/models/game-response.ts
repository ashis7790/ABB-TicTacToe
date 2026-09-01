export interface GameResponse {
    id: string;
    board: (number | null)[];
    currentPlayer: number;
    mode: number;
    status: number;
    winner: number | null;
    winningCells: number[];
    moves: Move[];
  }
  
  export interface Move {
    moveNumber: number;
    player: number;
    row: number;
    column: number;
  }