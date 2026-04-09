import { Injectable } from '@angular/core';

export interface MockUser {
  username: string;
  email: string;
  password: string;
}

@Injectable({
  providedIn: 'root'
})
export class Auth {
  private readonly usersKey = 'smarttasks_users';
  private readonly sessionKey = 'smarttasks_current_user';

  private getUsers(): MockUser[] {
    const raw = localStorage.getItem(this.usersKey);
    return raw ? JSON.parse(raw) as MockUser[] : [];
  }

  private saveUsers(users: MockUser[]): void {
    localStorage.setItem(this.usersKey, JSON.stringify(users));
  }

  register(user: MockUser): { success: boolean; message: string } {
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