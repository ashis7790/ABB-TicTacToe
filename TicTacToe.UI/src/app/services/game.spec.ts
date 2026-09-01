import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

import { GameResponse } from '../models/game-response';

@Injectable({
  providedIn: 'root'
})
export class GameService {

  private http = inject(HttpClient);

  private apiUrl = 'https://localhost:7257/api';

  createGame(mode: number): Observable<GameResponse> {
    return this.http.post<GameResponse>(
      `${this.apiUrl}/Games`,
      {
        mode: mode
      }
    );
  }

  getGame(gameId: string): Observable<GameResponse> {
    return this.http.get<GameResponse>(
      `${this.apiUrl}/Games/${gameId}`
    );
  }
}