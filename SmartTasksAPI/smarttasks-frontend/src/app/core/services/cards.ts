import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { CardItemModel } from '../models/card-item.model';

@Injectable({
  providedIn: 'root'
})
export class Cards {
  private readonly baseUrl = '/api';

  constructor(private http: HttpClient) { }

  getCardsByList(listId: string): Observable<CardItemModel[]> {
    return this.http.get<CardItemModel[]>(`${this.baseUrl}/lists/${listId}/cards`);
  }

  getCardById(cardId: string): Observable<CardItemModel> {
    return this.http.get<CardItemModel>(`${this.baseUrl}/cards/${cardId}`);
  }

  createCard(listId: string, payload: {
    title: string;
    description?: string | null;
    dueDateUtc?: string | null;
  }): Observable<CardItemModel> {
    return this.http.post<CardItemModel>(`${this.baseUrl}/lists/${listId}/cards`, payload);
  }

  updateCard(cardId: string, payload: {
    title: string;
    description?: string | null;
    position: number;
    dueDateUtc?: string | null;
  }): Observable<void> {
    return this.http.put<void>(`${this.baseUrl}/cards/${cardId}`, payload);
  }

  moveCard(cardId: string, payload: {
    targetListId: string;
    targetPosition: number;
  }): Observable<void> {
    return this.http.patch<void>(`${this.baseUrl}/cards/${cardId}/move`, payload);
  }

  assignUser(cardId: string, userId: string): Observable<void> {
    return this.http.post<void>(`${this.baseUrl}/cards/${cardId}/assignments/${userId}`, {});
  }

  unassignUser(cardId: string, userId: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/cards/${cardId}/assignments/${userId}`);
  }

  deleteCard(cardId: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/cards/${cardId}`);
  }
}