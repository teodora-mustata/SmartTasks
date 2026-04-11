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

  getBoardById(boardId: string): Observable<BoardModel> {
    return this.http.get<BoardModel>(`${this.apiUrl}/${boardId}`);
  }

  createBoard(payload: {
    name: string;
    description?: string | null;
    ownerId: string;
  }): Observable<BoardModel> {
    return this.http.post<BoardModel>(this.apiUrl, payload);
  }

  updateBoard(boardId: string, payload: {
    name: string;
    description?: string | null;
  }): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/${boardId}`, payload);
  }

  deleteBoard(boardId: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${boardId}`);
  }

  addMember(boardId: string, payload: { userId: string }): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/${boardId}/members`, payload);
  }

  removeMember(boardId: string, userId: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${boardId}/members/${userId}`);
  }
}