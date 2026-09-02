import {
  Component,
  OnInit,
  inject,
  ChangeDetectorRef
} from '@angular/core';

import { FormsModule } from '@angular/forms';

import {
  GameService,
  ScoreboardResponse
} from './services/game';

import {
  Move,
  GameResponse
} from './models/game-response';

@Component({
  selector: 'app-root',
  templateUrl: './app.html',
  styleUrl: './app.css',
  imports: [FormsModule]
})
export class App implements OnInit {

  private gameService = inject(GameService);
  private cdr = inject(ChangeDetectorRef);

  board: (string | null)[] = Array(9).fill(null);

  currentPlayer = 'X';

  status = 'In Progress';

  errorMessage = '';

  winner: string | null = null;

  winningCells: number[] = [];

  // GAME MODE
  // 0 = Player vs Player
  // 1 = Player vs Computer
  selectedMode = 0;

  // SCOREBOARD
  xWins = 0;
  oWins = 0;
  draws = 0;

  // MOVE HISTORY
  moves: Move[] = [];

  // GAME
  gameId: string | null = null;

  gameReady = false;

  moveInProgress = false;

  ngOnInit(): void {
    this.createGame();
    this.loadScoreboard();
  }

  createGame(): void {

    this.gameReady = false;
    this.gameId = null;
    this.moveInProgress = false;
    this.errorMessage = '';

    this.gameService.createGame(
      this.selectedMode
    ).subscribe({

      next: (game) => {

        console.log(
          'Game created:',
          game
        );

        this.updateGameFromApi(game);

        console.log(
          'Game ID:',
          this.gameId
        );

        console.log(
          'Game Ready:',
          this.gameReady
        );
      },

      error: (error) => {

        console.error(
          'Failed to create game:',
          error
        );

        this.gameReady = false;

        this.errorMessage =
          error?.error?.message ||
          error?.error ||
          'Unable to create the game.';

        this.cdr.detectChanges();
      }
    });
  }

  changeGameMode(): void {

    if (this.moveInProgress) {
      return;
    }

    console.log(
      'Changing game mode to:',
      this.selectedMode
    );

    // Changing mode starts a fresh game.
    this.createGame();
  }

  makeMove(index: number): void {

    console.log(
      'CELL CLICKED:',
      index,
      'gameId:',
      this.gameId,
      'ready:',
      this.gameReady,
      'inProgress:',
      this.moveInProgress
    );

    if (
      !this.gameReady ||
      !this.gameId ||
      this.moveInProgress
    ) {
      console.log('MOVE BLOCKED');
      return;
    }

    // In computer mode, the human controls only X.
    if (
      this.selectedMode === 1 &&
      this.currentPlayer !== 'X'
    ) {
      console.log(
        'MOVE BLOCKED - Computer turn'
      );

      return;
    }

    // Do not allow an occupied cell.
    if (this.board[index] !== null) {

      console.log(
        'MOVE BLOCKED - Cell occupied'
      );

      return;
    }

    const row =
      Math.floor(index / 3);

    const column =
      index % 3;

    this.errorMessage = '';

    this.moveInProgress = true;

    this.cdr.detectChanges();

    console.log(
      'Calling API:',
      this.gameId,
      'row:',
      row,
      'column:',
      column
    );

    this.gameService.makeMove(
      this.gameId,
      row,
      column
    ).subscribe({

      next: (game) => {

        console.log(
          'MOVE RESPONSE:',
          game
        );

        /*
         * In computer mode the backend response already
         * contains both the human X move and the
         * computer O move.
         */
        this.updateGameFromApi(game);

        this.moveInProgress = false;

        this.loadScoreboard();

        this.cdr.detectChanges();
      },

      error: (error) => {

        console.error(
          'Move failed:',
          error
        );

        this.errorMessage =
          error?.error?.message ||
          error?.error ||
          'Unable to make the move.';

        this.moveInProgress = false;

        this.cdr.detectChanges();
      }
    });
  }

  resetGame(): void {

    if (
      !this.gameId ||
      this.moveInProgress
    ) {
      return;
    }

    this.moveInProgress = true;

    this.errorMessage = '';

    this.gameService.resetGame(
      this.gameId
    ).subscribe({

      next: (game) => {

        console.log(
          'Game reset:',
          game
        );

        this.updateGameFromApi(game);

        this.errorMessage = '';

        this.moveInProgress = false;

        this.cdr.detectChanges();
      },

      error: (error) => {

        console.error(
          'Reset failed:',
          error
        );

        this.errorMessage =
          error?.error?.message ||
          error?.error ||
          'Unable to reset the game.';

        this.moveInProgress = false;

        this.cdr.detectChanges();
      }
    });
  }

  undoMove(): void {

    if (
      !this.gameId ||
      this.moveInProgress
    ) {
      return;
    }

    this.errorMessage = '';

    this.moveInProgress = true;

    this.gameService.undoGame(
      this.gameId
    ).subscribe({

      next: (game) => {

        console.log(
          'Undo response:',
          game
        );

        this.updateGameFromApi(game);

        this.moveInProgress = false;

        this.cdr.detectChanges();
      },

      error: (error) => {

        console.error(
          'Undo failed:',
          error
        );

        this.errorMessage =
          error?.error?.message ||
          error?.error ||
          'Unable to undo the move.';

        this.moveInProgress = false;

        this.cdr.detectChanges();
      }
    });
  }

  loadScoreboard(): void {

    this.gameService.getScoreboard().subscribe({

      next: (
        scoreboard: ScoreboardResponse
      ) => {

        console.log(
          'Scoreboard:',
          scoreboard
        );

        this.xWins =
          scoreboard.xWins;

        this.oWins =
          scoreboard.oWins;

        this.draws =
          scoreboard.draws;

        this.cdr.detectChanges();
      },

      error: (error) => {

        console.error(
          'Failed to load scoreboard:',
          error
        );
      }
    });
  }

  resetScoreboard(): void {

    if (this.moveInProgress) {
      return;
    }

    this.errorMessage = '';

    this.moveInProgress = true;

    this.gameService.resetScoreboard().subscribe({

      next: (
        scoreboard: ScoreboardResponse
      ) => {

        console.log(
          'Scoreboard reset:',
          scoreboard
        );

        this.xWins =
          scoreboard.xWins;

        this.oWins =
          scoreboard.oWins;

        this.draws =
          scoreboard.draws;

        this.moveInProgress = false;

        this.cdr.detectChanges();
      },

      error: (error) => {

        console.error(
          'Reset scoreboard failed:',
          error
        );

        this.errorMessage =
          error?.error?.message ||
          error?.error ||
          'Unable to reset scoreboard.';

        this.moveInProgress = false;

        this.cdr.detectChanges();
      }
    });
  }

  private updateGameFromApi(
    game: GameResponse
  ): void {

    console.log(
      'Updating UI from API:',
      game
    );

    this.gameId =
      game.id;

    this.selectedMode =
      game.mode;

    this.board =
      game.board.map(
        cell => {

          if (cell === null) {
            return null;
          }

          return cell === 0
            ? 'X'
            : 'O';
        }
      );

    this.currentPlayer =
      game.currentPlayer === 0
        ? 'X'
        : 'O';

    this.winningCells =
      game.winningCells ?? [];

    this.winner =
      game.winner === null
        ? null
        : game.winner === 0
          ? 'X'
          : 'O';

    this.status =
      game.status === 0
        ? 'In Progress'
        : game.status === 1 ||
          game.status === 2
          ? 'Won'
          : 'Draw';

    this.moves =
      game.moves ?? [];

    this.gameReady = true;
  }
}