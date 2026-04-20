import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { forkJoin } from 'rxjs';

import { BoardModel } from '../../core/models/board.model';
import { Boards as BoardsService } from '../../core/services/boards';
import { Lists as ListsService } from '../../core/services/lists';
import { Auth, AuthUser } from '../../core/services/auth';
import { BoardAccess } from '../../core/services/board-access';

@Component({
  selector: 'app-boards',
  imports: [CommonModule, FormsModule],
  templateUrl: './boards.html',
  styleUrl: './boards.scss',
})
export class Boards implements OnInit {
  boards: BoardModel[] = [];
  isLoading = true;
  errorMessage = '';

  currentBackendUser: AuthUser | null = null;
  resolvingCurrentUser = true;

  newBoardName = '';
  newBoardDescription = '';
  creatingBoard = false;
  createBoardError = '';

  editingBoardId: string | null = null;
  editBoardById: Record<string, { name: string; description: string }> = {};
  savingBoardId: string | null = null;
  editErrorByBoardId: Record<string, string> = {};

  deletingBoardId: string | null = null;

  readonly defaultLists = [
    'Backlog',
    'Ready',
    'In progress',
    'In review',
    'Done'
  ];

  constructor(
    private boardsService: BoardsService,
    private listsService: ListsService,
    private boardAccess: BoardAccess,
    private authService: Auth,
    private router: Router
  ) { }

  ngOnInit(): void {
    this.boardAccess.getCurrentBackendUser().subscribe({
      next: (user) => {
        this.currentBackendUser = user;
        this.resolvingCurrentUser = false;
        this.loadBoards();
      },
      error: () => {
        this.errorMessage = 'Failed to resolve current user.';
        this.resolvingCurrentUser = false;
        this.isLoading = false;
      }
    });
  }

  logout(): void {
    this.authService.logout();
    this.router.navigateByUrl('/login');
  }

  loadBoards(): void {
    this.isLoading = true;
    this.errorMessage = '';

    this.boardsService.getBoards().subscribe({
      next: (data) => {
        this.boards = data.filter(board =>
          this.boardAccess.canAccessBoard(board, this.currentBackendUser?.id)
        );

        for (const board of this.boards) {
          this.editBoardById[board.id] = {
            name: board.name,
            description: board.description ?? ''
          };
        }

        this.isLoading = false;
      },
      error: () => {
        this.errorMessage = 'Failed to load boards.';
        this.isLoading = false;
      }
    });
  }

  openBoard(boardId: string): void {
    this.router.navigate(['/boards', boardId]);
  }

  isBoardOwner(board: BoardModel): boolean {
    return !!this.currentBackendUser && board.ownerId === this.currentBackendUser.id;
  }

  createBoard(): void {
    const trimmedName = this.newBoardName.trim();
    const trimmedDescription = this.newBoardDescription.trim();

    this.createBoardError = '';

    if (!this.currentBackendUser) {
      this.createBoardError = 'Current user is not available.';
      return;
    }

    if (!trimmedName) {
      this.createBoardError = 'Board name is required.';
      return;
    }

    this.creatingBoard = true;

    this.boardsService.createBoard({
      name: trimmedName,
      description: trimmedDescription || null,
      ownerId: this.currentBackendUser.id
    }).subscribe({
      next: (createdBoard) => {
        forkJoin(
          this.defaultLists.map(listName =>
            this.listsService.createList(createdBoard.id, listName)
          )
        ).subscribe({
          next: () => {
            this.newBoardName = '';
            this.newBoardDescription = '';
            this.creatingBoard = false;
            this.loadBoards();
          },
          error: () => {
            this.createBoardError = 'Board was created, but default columns could not be created.';
            this.creatingBoard = false;
            this.loadBoards();
          }
        });
      },
      error: () => {
        this.createBoardError = 'Failed to create board.';
        this.creatingBoard = false;
      }
    });
  }

  startEditBoard(board: BoardModel, event: MouseEvent): void {
    event.stopPropagation();

    this.editingBoardId = board.id;
    this.editErrorByBoardId[board.id] = '';

    this.editBoardById[board.id] = {
      name: board.name,
      description: board.description ?? ''
    };
  }

  cancelEditBoard(board: BoardModel, event?: MouseEvent): void {
    event?.stopPropagation();

    this.editingBoardId = null;
    this.editErrorByBoardId[board.id] = '';

    this.editBoardById[board.id] = {
      name: board.name,
      description: board.description ?? ''
    };
  }

  saveEditBoard(board: BoardModel, event: MouseEvent): void {
    event.stopPropagation();

    const form = this.editBoardById[board.id];
    const trimmedName = form.name.trim();
    const trimmedDescription = form.description.trim();

    this.editErrorByBoardId[board.id] = '';

    if (!trimmedName) {
      this.editErrorByBoardId[board.id] = 'Board name is required.';
      return;
    }

    this.savingBoardId = board.id;

    this.boardsService.updateBoard(board.id, {
      name: trimmedName,
      description: trimmedDescription || null
    }).subscribe({
      next: () => {
        this.savingBoardId = null;
        this.editingBoardId = null;
        this.loadBoards();
      },
      error: () => {
        this.editErrorByBoardId[board.id] = 'Failed to update board.';
        this.savingBoardId = null;
      }
    });
  }

  deleteBoard(boardId: string, event: MouseEvent): void {
    event.stopPropagation();

    const shouldDelete = window.confirm('Are you sure you want to delete this board?');
    if (!shouldDelete) {
      return;
    }

    this.deletingBoardId = boardId;

    this.boardsService.deleteBoard(boardId).subscribe({
      next: () => {
        this.deletingBoardId = null;
        this.loadBoards();
      },
      error: () => {
        this.errorMessage = 'Failed to delete board.';
        this.deletingBoardId = null;
      }
    });
  }
}