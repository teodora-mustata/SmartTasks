import { Routes } from '@angular/router';
import { Login } from './pages/login/login';
import { Register } from './pages/register/register';
import { Boards } from './pages/boards/boards';
import { BoardDetails } from './pages/board-details/board-details';
import { CardDetails } from './pages/card-details/card-details';
import { CreateCard } from './pages/create-card/create-card';

export const routes: Routes = [
    { path: '', redirectTo: 'login', pathMatch: 'full' },
    { path: 'login', component: Login },
    { path: 'register', component: Register },
    { path: 'boards', component: Boards },
    { path: 'boards/:id', component: BoardDetails },
    { path: 'boards/:boardId/cards/new', component: CreateCard },
    { path: 'boards/:boardId/cards/:cardId', component: CardDetails },
    { path: '**', redirectTo: 'login' }
];