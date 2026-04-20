import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';

import { BoardModel } from '../../core/models/board.model';
import { BoardListModel } from '../../core/models/board-list.model';

import { Boards as BoardsService } from '../../core/services/boards';
import { Cards as CardsService } from '../../core/services/cards';
import { BoardAccess } from '../../core/services/board-access';
import { AuthUser } from '../../core/services/auth';

@Component({
  selector: 'app-create-card',
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './create-card.html',
  styleUrl: './create-card.scss',
})
export class CreateCard implements OnInit {
  boardId = '';
  board: BoardModel | null = null;
  backlogList: BoardListModel | null = null;

  currentBackendUser: AuthUser | null = null;

  isLoading = true;
  errorMessage = '';

  title = '';
  description = '';
  dueDate = '';
  creatingCard = false;
  createCardError = '';

  readonly minDate = this.getTodayAsInputDate();

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private boardsService: BoardsService,
    private cardsService: CardsService,
    private boardAccess: BoardAccess
  ) { }

  ngOnInit(): void {
    this.boardId = this.route.snapshot.paramMap.get('boardId') ?? '';

    if (!this.boardId) {
      this.errorMessage = 'Board id is missing.';
      this.isLoading = false;
      return;
    }

    this.boardAccess.getCurrentBackendUser().subscribe({
      next: (user) => {
        this.currentBackendUser = user;

        if (!user) {
          this.router.navigate(['/boards']);
          return;
        }

        this.loadBoard();
      },
      error: () => {
        this.errorMessage = 'Failed to resolve current user.';
        this.isLoading = false;
      }
    });
  }

  private getTodayAsInputDate(): string {
    const now = new Date();
    const offset = now.getTimezoneOffset();
    return new Date(now.getTime() - offset * 60000).toISOString().split('T')[0];
  }

  private loadBoard(): void {
    this.isLoading = true;
    this.errorMessage = '';

    this.boardsService.getBoardById(this.boardId).subscribe({
      next: (board) => {
        if (!this.boardAccess.canAccessBoard(board, this.currentBackendUser?.id)) {
          this.router.navigate(['/boards']);
          return;
        }

        this.board = board;

        const lists = board.lists ?? [];
        this.backlogList =
          lists.find(l => l.name.trim().toLowerCase() === 'backlog') ?? null;

        if (!this.backlogList) {
          this.errorMessage = 'Backlog column was not found for this board.';
        }

        this.isLoading = false;
      },
      error: () => {
        this.errorMessage = 'Failed to load board.';
        this.isLoading = false;
      }
    });
  }

  createCard(): void {
    const trimmedTitle = this.title.trim();
    const trimmedDescription = this.description.trim();

    this.createCardError = '';

    if (!this.backlogList) {
      this.createCardError = 'Backlog column was not found.';
      return;
    }

    if (!trimmedTitle) {
      this.createCardError = 'Card title is required.';
      return;
    }

    if (this.dueDate && this.dueDate < this.minDate) {
      this.createCardError = 'Due date cannot be in the past.';
      return;
    }

    this.creatingCard = true;

    this.cardsService.createCard(this.backlogList.id, {
      title: trimmedTitle,
      description: trimmedDescription || null,
      dueDateUtc: this.dueDate ? new Date(this.dueDate).toISOString() : null
    }).subscribe({
      next: () => {
        this.creatingCard = false;
        this.router.navigate(['/boards', this.boardId]);
      },
      error: () => {
        this.createCardError = 'Failed to create card.';
        this.creatingCard = false;
      }
    });
  }
}