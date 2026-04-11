import { Injectable } from '@angular/core';
import { Observable, of, switchMap } from 'rxjs';

import { Auth } from './auth';
import { Users, BackendUser } from './users';
import { BoardModel } from '../models/board.model';

@Injectable({
    providedIn: 'root'
})
export class BoardAccess {
    constructor(
        private authService: Auth,
        private usersService: Users
    ) { }

    getCurrentBackendUser(): Observable<BackendUser | null> {
        const currentUser = this.authService.getCurrentUser();

        if (!currentUser) {
            return of(null);
        }

        return this.usersService.getUsers().pipe(
            switchMap(users => {
                const existingUser = users.find(
                    u => u.email.trim().toLowerCase() === currentUser.email.trim().toLowerCase()
                );

                if (existingUser) {
                    return of(existingUser);
                }

                return this.usersService.createUser({
                    fullName: currentUser.username,
                    email: currentUser.email
                });
            })
        );
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