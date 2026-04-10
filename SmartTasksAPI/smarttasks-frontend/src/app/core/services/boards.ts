import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { BoardModel } from '../models/board.model';

@Injectable({
  providedIn: 'root'
})
export class Boards {
  private readonly apiUrl = 'http://localhost:5065/api/boards';

  constructor(private http: HttpClient) { }

  getBoards(): Observable<BoardModel[]> {
    return this.http.get<BoardModel[]>(this.apiUrl);
  }
}
