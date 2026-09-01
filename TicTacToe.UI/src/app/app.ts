import {
  Component,
  OnInit,
  inject,
  ChangeDetectorRef
} from '@angular/core';

import { GameService } from './services/game';
import { Move, GameResponse } from './models/game-response';

@Component({
  selector: 'app-root',
  templateUrl: './app.html',
  styleUrl: './app.css'
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

  xWins = 0;

  oWins = 0;

  draws = 0;

  moves: Move[] = [];

  gameId: string | null = null;

  gameReady = false;

  moveInProgress = false;

  private history: (string | null)[][] = [];


  // ==========================================
  // INIT
  // ==========================================

  ngOnInit(): void {
    this.createGame();
  }


  // ==========================================
  // CREATE GAME
  // ==========================================

  createGame(): void {

    this.gameReady = false;
    this.gameId = null;
    this.moveInProgress = false;
    this.errorMessage = '';

    this.gameService.createGame(0).subscribe({

      next: (game) => {

        console.log('Game created:', game);

        this.updateGameFromApi(game);

        this.gameReady = true;

        this.cdr.detectChanges();

        console.log('Game ID:', this.gameId);
        console.log('Game Ready:', this.gameReady);
      },

      error: (error) => {

        console.error('Failed to create game:', error);

        this.gameReady = false;

        this.errorMessage =
          error?.error?.message ||
          error?.error ||
          'Unable to create the game.';

        this.cdr.detectChanges();
      }

    });
  }


  // ==========================================
  // MAKE MOVE
  // ==========================================

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


    const row = Math.floor(index / 3);

    const column = index % 3;


    this.errorMessage = '';

    this.moveInProgress = true;


    // Immediately refresh UI
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

        console.log('MOVE RESPONSE:', game);


        // Update board from backend
        this.updateGameFromApi(game);


        this.moveInProgress = false;


        // Force Angular UI refresh
        this.cdr.detectChanges();


        console.log(
          'Board after move:',
          this.board
        );

        console.log(
          'Current player:',
          this.currentPlayer
        );
      },


      error: (error) => {

        console.error('Move failed:', error);


        this.errorMessage =
          error?.error?.message ||
          error?.error ||
          'Unable to make the move.';


        this.moveInProgress = false;


        this.cdr.detectChanges();
      }

    });
  }


  // ==========================================
  // RESET GAME
  // ==========================================

  resetGame(): void {

    if (
      !this.gameId ||
      this.moveInProgress
    ) {
      return;
    }


    this.moveInProgress = true;

    this.errorMessage = '';


    this.cdr.detectChanges();


    this.gameService.resetGame(
      this.gameId
    ).subscribe({

      next: (game) => {

        console.log(
          'Game reset:',
          game
        );


        this.updateGameFromApi(game);

        this.history = [];

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


  // ==========================================
  // UNDO MOVE
  // ==========================================

  undoMove(): void {

    if (
      !this.gameId ||
      this.moveInProgress
    ) {
      return;
    }


    this.errorMessage = '';

    this.moveInProgress = true;


    this.cdr.detectChanges();


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


  // ==========================================
  // UPDATE UI FROM API
  // ==========================================

  private updateGameFromApi(
    game: GameResponse
  ): void {

    console.log(
      'Updating UI from API:',
      game
    );


    // Game ID
    this.gameId = game.id;


    // Board
    this.board = game.board.map(
      cell => {

        if (cell === null) {
          return null;
        }

        return cell === 0
          ? 'X'
          : 'O';
      }
    );


    // Current player
    this.currentPlayer =
      game.currentPlayer === 0
        ? 'X'
        : 'O';


    // Winning cells
    this.winningCells =
      game.winningCells ?? [];


    // Winner
    this.winner =
      game.winner === null
        ? null
        : game.winner === 0
          ? 'X'
          : 'O';


    // Status
    this.status =
      game.status === 0
        ? 'In Progress'
        : game.status === 1
          ? 'Won'
          : 'Draw';


    // Move history
    this.moves =
      game.moves ?? [];


    // Game is ready
    this.gameReady = true;
  }

}