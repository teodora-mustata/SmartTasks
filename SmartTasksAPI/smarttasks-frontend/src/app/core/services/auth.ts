import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { firstValueFrom } from 'rxjs';

export interface MockUser {
  username: string;
  email: string;
  password: string;
}

@Injectable({
  providedIn: 'root'
})
export class Auth {
  constructor(private http: HttpClient) {}
  private readonly usersKey = 'smarttasks_users';
  private readonly sessionKey = 'smarttasks_current_user';

  private getUsers(): MockUser[] {
    const raw = localStorage.getItem(this.usersKey);
    return raw ? JSON.parse(raw) as MockUser[] : [];
  }

  private saveUsers(users: MockUser[]): void {
    localStorage.setItem(this.usersKey, JSON.stringify(users));
  }
  async register(user: MockUser): Promise<{ success: boolean; message: string }> {
    const users = this.getUsers();

    const usernameExists = users.some(
      u => u.username.trim().toLowerCase() === user.username.trim().toLowerCase()
    );

    if (usernameExists) {
      return {
        success: false,
        message: 'Username already exists.'
      };
    }

    const emailExists = users.some(
      u => u.email.trim().toLowerCase() === user.email.trim().toLowerCase()
    );

    if (emailExists) {
      return {
        success: false,
        message: 'An account with this email already exists.'
      };
    }

    // Attempt to persist the user to the backend. Backend expects { fullName, email }.
    try {
      await firstValueFrom(this.http.post('/api/users', { fullName: user.username.trim(), email: user.email.trim().toLowerCase() }));
    } catch (err) {
      // If backend call fails, continue and save locally so UX isn't blocked.
      // This preserves existing local-auth behavior while ensuring server persistence when available.
      console.warn('Failed to persist user to API:', err);
    }

    users.push({
      username: user.username.trim(),
      email: user.email.trim().toLowerCase(),
      password: user.password
    });

    this.saveUsers(users);

    return {
      success: true,
      message: 'Account created successfully.'
    };
  }

  login(username: string, password: string): { success: boolean; message: string } {
    const users = this.getUsers();

    const foundUser = users.find(
      u =>
        u.username.trim().toLowerCase() === username.trim().toLowerCase() &&
        u.password === password
    );

    if (!foundUser) {
      return {
        success: false,
        message: 'Invalid credentials.'
      };
    }

    localStorage.setItem(this.sessionKey, JSON.stringify(foundUser));

    return {
      success: true,
      message: 'Login successful.'
    };
  }

  logout(): void {
    localStorage.removeItem(this.sessionKey);
  }

  getCurrentUser(): MockUser | null {
    const raw = localStorage.getItem(this.sessionKey);
    return raw ? JSON.parse(raw) as MockUser : null;
  }

  isLoggedIn(): boolean {
    return this.getCurrentUser() !== null;
  }
}
