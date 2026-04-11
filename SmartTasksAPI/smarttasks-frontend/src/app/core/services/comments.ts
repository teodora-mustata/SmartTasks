import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { CardCommentModel } from '../models/card-comment.model';

@Injectable({
  providedIn: 'root'
})
export class Comments {
  private readonly baseUrl = 'http://localhost:5065/api';

  constructor(private http: HttpClient) { }

  getCommentsByCard(cardId: string): Observable<CardCommentModel[]> {
    return this.http.get<CardCommentModel[]>(`${this.baseUrl}/cards/${cardId}/comments`);
  }

  createComment(cardId: string, payload: {
    authorId: string;
    message: string;
  }): Observable<CardCommentModel> {
    return this.http.post<CardCommentModel>(`${this.baseUrl}/cards/${cardId}/comments`, payload);
  }

  deleteComment(commentId: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/comments/${commentId}`);
  }
}