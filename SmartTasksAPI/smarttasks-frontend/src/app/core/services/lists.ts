import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { BoardListModel } from '../models/board-list.model';

@Injectable({
  providedIn: 'root'
})
export class Lists {
  private readonly baseUrl = 'http://localhost:5065/api';

  constructor(private http: HttpClient) { }

  createList(boardId: string, name: string): Observable<BoardListModel> {
    return this.http.post<BoardListModel>(`${this.baseUrl}/boards/${boardId}/lists`, {
      name
    });
  }

  getListById(listId: string): Observable<BoardListModel> {
    return this.http.get<BoardListModel>(`${this.baseUrl}/lists/${listId}`);
  }

  updateList(listId: string, payload: { name: string; position: number }): Observable<void> {
    return this.http.put<void>(`${this.baseUrl}/lists/${listId}`, payload);
  }

  deleteList(listId: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/lists/${listId}`);
  }
}