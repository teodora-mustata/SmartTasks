import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface BackendUser {
  id: string;
  fullName: string;
  email: string;
  createdAtUtc?: string;
}

@Injectable({
  providedIn: 'root'
})
export class Users {
  private readonly apiUrl = 'http://localhost:5065/api/users';

  constructor(private http: HttpClient) { }

  getUsers(): Observable<BackendUser[]> {
    return this.http.get<BackendUser[]>(this.apiUrl);
  }

  createUser(payload: { fullName: string; email: string }): Observable<BackendUser> {
    return this.http.post<BackendUser>(this.apiUrl, payload);
  }
}