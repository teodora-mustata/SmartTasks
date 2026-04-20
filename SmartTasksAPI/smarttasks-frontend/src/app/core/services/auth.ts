import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { firstValueFrom } from 'rxjs';

export interface AuthUser {
  id: string;
  fullName: string;
  email: string;
  createdAtUtc: string;
}

export interface AuthResponse {
  token: string;
  user: AuthUser;
}

@Injectable({
  providedIn: 'root'
})
export class Auth {
  private readonly baseUrl = '/api/auth';
  private readonly tokenKey = 'smarttasks_token';
  private readonly userKey = 'smarttasks_user';

  constructor(private http: HttpClient) { }

  async register(payload: {
    fullName: string;
    email: string;
    password: string;
  }): Promise<{ success: boolean; message: string }> {
    try {
      const response = await firstValueFrom(
        this.http.post<AuthResponse>(`${this.baseUrl}/register`, payload)
      );

      this.saveSession(response);
      return {
        success: true,
        message: 'Account created successfully.'
      };
    } catch (err: any) {
      const backendMessage = err?.error?.message;
      return {
        success: false,
        message: backendMessage || 'Registration failed.'
      };
    }
  }

  async login(identifier: string, password: string): Promise<{ success: boolean; message: string }> {
    try {
      const response = await firstValueFrom(
        this.http.post<AuthResponse>(`${this.baseUrl}/login`, {
          identifier,
          password
        })
      );

      this.saveSession(response);
      return {
        success: true,
        message: 'Login successful.'
      };
    } catch (err: any) {
      const backendMessage = err?.error?.message;
      return {
        success: false,
        message: backendMessage || 'Invalid credentials.'
      };
    }
  }

  logout(): void {
    localStorage.removeItem(this.tokenKey);
    localStorage.removeItem(this.userKey);
  }

  getToken(): string | null {
    return localStorage.getItem(this.tokenKey);
  }

  getCurrentUser(): AuthUser | null {
    const raw = localStorage.getItem(this.userKey);
    return raw ? JSON.parse(raw) as AuthUser : null;
  }

  isLoggedIn(): boolean {
    return !!this.getToken() && !!this.getCurrentUser();
  }

  private saveSession(response: AuthResponse): void {
    localStorage.setItem(this.tokenKey, response.token);
    localStorage.setItem(this.userKey, JSON.stringify(response.user));
  }
}