import { Injectable } from '@angular/core';
import { Observable, of } from 'rxjs';

import { Auth, AuthUser } from './auth';
import { BoardModel } from '../models/board.model';

@Injectable({
    providedIn: 'root'
})
export class BoardAccess {
    constructor(private authService: Auth) { }

    getCurrentBackendUser(): Observable<AuthUser | null> {
        return of(this.authService.getCurrentUser());
    }

    canAccessBoard(board: BoardModel | null | undefined, userId: string | null | undefined): boolean {
        if (!board || !userId) {
            return false;
        }

        if (board.ownerId === userId) {
            return true;
        }

        return (board.members ?? []).some(member => member.userId === userId);
    }
}