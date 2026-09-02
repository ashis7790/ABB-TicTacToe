import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

import { GameResponse } from '../models/game-response';

export interface ScoreboardResponse {
  xWins: number;
  oWins: number;
  draws: number;
}

@Injectable({
  providedIn: 'root'
})
export class GameService {

  private http = inject(HttpClient);

  private apiUrl = 'https://localhost:7257/api';


  // ==========================================
  // CREATE GAME
  // ==========================================

  createGame(mode: number): Observable<GameResponse> {

    return this.http.post<GameResponse>(
      `${this.apiUrl}/Games`,
      {
        mode: mode
      }
    );
  }


  // ==========================================
  // GET GAME
  // ==========================================

  getGame(gameId: string): Observable<GameResponse> {

    return this.http.get<GameResponse>(
      `${this.apiUrl}/Games/${gameId}`
    );
  }


  // ==========================================
  // MAKE MOVE
  // ==========================================

  makeMove(
    gameId: string,
    row: number,
    column: number
  ): Observable<GameResponse> {

    return this.http.post<GameResponse>(
      `${this.apiUrl}/Games/${gameId}/moves`,
      {
        row: row,
        column: column
      }
    );
  }


  // ==========================================
  // RESET GAME
  // ==========================================

  resetGame(gameId: string): Observable<GameResponse> {

    return this.http.post<GameResponse>(
      `${this.apiUrl}/Games/${gameId}/reset`,
      {}
    );
  }


  // ==========================================
  // UNDO MOVE
  // ==========================================

  undoGame(gameId: string): Observable<GameResponse> {

    return this.http.post<GameResponse>(
      `${this.apiUrl}/Games/${gameId}/undo`,
      {}
    );
  }


  // ==========================================
  // GET SCOREBOARD
  // ==========================================

 // GET SCOREBOARD
getScoreboard(): Observable<ScoreboardResponse> {
  return this.http.get<ScoreboardResponse>(
    `${this.apiUrl}/Games/scoreboard`
  );
}

// RESET SCOREBOARD
resetScoreboard(): Observable<ScoreboardResponse> {
  return this.http.post<ScoreboardResponse>(
    `${this.apiUrl}/Games/scoreboard/reset`,
    {}
  );
}

}