import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { forkJoin } from 'rxjs';

import { CardItemModel, CardAssignmentModel } from '../../core/models/card-item.model';
import { CardCommentModel } from '../../core/models/card-comment.model';
import { BoardListModel } from '../../core/models/board-list.model';
import { BoardModel } from '../../core/models/board.model';

import { Cards as CardsService } from '../../core/services/cards';
import { Comments as CommentsService } from '../../core/services/comments';
import { Boards as BoardsService } from '../../core/services/boards';
import { AuthUser } from '../../core/services/auth';
import { BackendUser } from '../../core/services/users';
import { BoardAccess } from '../../core/services/board-access';

interface MoveTargetOption {
  id: string;
  name: string;
  cardCount: number;
}

@Component({
  selector: 'app-card-details',
  imports: [CommonModule, RouterLink, FormsModule],
  templateUrl: './card-details.html',
  styleUrl: './card-details.scss',
})
export class CardDetails implements OnInit {
  card: CardItemModel | null = null;
  comments: CardCommentModel[] = [];
  isLoading = true;
  errorMessage = '';

  currentBackendUser: AuthUser | null = null;

  boardId = '';
  cardId = '';

  moveTargets: MoveTargetOption[] = [];
  selectedTargetListId = '';
  movingCard = false;
  moveCardError = '';

  boardMembers: BackendUser[] = [];
  selectedAssigneeId = '';
  assignError = '';
  assigningUser = false;
  unassigningUserId: string | null = null;

  isEditingCard = false;
  editTitle = '';
  editDescription = '';
  editDueDate = '';
  savingCard = false;
  editCardError = '';

  newCommentMessage = '';
  creatingComment = false;
  createCommentError = '';

  deletingCard = false;
  deletingCommentId: string | null = null;

  readonly minDate = this.getTodayAsInputDate();

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private cardsService: CardsService,
    private commentsService: CommentsService,
    private boardsService: BoardsService,
    private boardAccess: BoardAccess
  ) { }

  ngOnInit(): void {
    this.boardId = this.route.snapshot.paramMap.get('boardId') ?? '';
    this.cardId = this.route.snapshot.paramMap.get('cardId') ?? '';

    if (!this.cardId) {
      this.errorMessage = 'Card id is missing.';
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

        if (this.boardId) {
          this.loadBoardContext(this.boardId);
        }

        this.loadCardDetails(this.cardId);
      },
      error: () => {
        this.errorMessage = 'Failed to resolve current user.';
        this.isLoading = false;
      }
    });
  }

  get availableAssignees(): BackendUser[] {
    const assignedIds = new Set((this.card?.assignments ?? []).map(a => a.userId));
    return this.boardMembers.filter(member => !assignedIds.has(member.id));
  }

  private getTodayAsInputDate(): string {
    const now = new Date();
    const offset = now.getTimezoneOffset();
    return new Date(now.getTime() - offset * 60000).toISOString().split('T')[0];
  }

  private getInputDateValue(dateString?: string | null): string {
    if (!dateString) {
      return '';
    }

    const date = new Date(dateString);
    const offset = date.getTimezoneOffset();
    return new Date(date.getTime() - offset * 60000).toISOString().split('T')[0];
  }

  private getStartOfToday(): Date {
    const today = new Date();
    today.setHours(0, 0, 0, 0);
    return today;
  }

  private getStartOfDate(dateString: string): Date {
    const date = new Date(dateString);
    date.setHours(0, 0, 0, 0);
    return date;
  }

  getDueDateStatus(card: CardItemModel): 'none' | 'overdue' | 'today' | 'soon' | 'later' {
    if (!card.dueDateUtc) {
      return 'none';
    }

    const today = this.getStartOfToday().getTime();
    const dueDate = this.getStartOfDate(card.dueDateUtc).getTime();
    const diffDays = Math.floor((dueDate - today) / (1000 * 60 * 60 * 24));

    if (diffDays < 0) return 'overdue';
    if (diffDays === 0) return 'today';
    if (diffDays <= 7) return 'soon';
    return 'later';
  }

  getDueDateLabel(card: CardItemModel): string {
    const status = this.getDueDateStatus(card);

    switch (status) {
      case 'overdue':
        return 'Overdue';
      case 'today':
        return 'Due today';
      case 'soon':
        return 'Due soon';
      case 'later':
        return 'Upcoming';
      default:
        return 'No due date';
    }
  }

  getAssignmentName(assignment: CardAssignmentModel): string {
    return (
      assignment.user?.fullName ||
      this.boardMembers.find(m => m.id === assignment.userId)?.fullName ||
      'Unknown user'
    );
  }

  private loadBoardContext(boardId: string): void {
    this.boardsService.getBoardById(boardId).subscribe({
      next: (board: BoardModel) => {
        if (!this.boardAccess.canAccessBoard(board, this.currentBackendUser?.id)) {
          this.router.navigate(['/boards']);
          return;
        }

        this.boardMembers = (board.members ?? [])
          .map(member => member.user)
          .filter((user): user is BackendUser => !!user);

        const systemOrder = ['Backlog', 'Ready', 'In progress', 'In review', 'Done'];

        const orderedLists = systemOrder
          .map(name => (board.lists ?? []).find(
            l => l.name.trim().toLowerCase() === name.trim().toLowerCase()
          ))
          .filter((list): list is BoardListModel => !!list);

        if (orderedLists.length === 0) {
          this.moveTargets = [];
          return;
        }

        forkJoin(
          orderedLists.map(list => this.cardsService.getCardsByList(list.id))
        ).subscribe({
          next: (cardsByList) => {
            this.moveTargets = orderedLists.map((list, index) => ({
              id: list.id,
              name: list.name,
              cardCount: cardsByList[index]?.length ?? 0
            }));
          },
          error: () => {
            this.moveCardError = 'Failed to load board columns.';
          }
        });
      },
      error: () => {
        this.moveCardError = 'Failed to load board columns.';
      }
    });
  }

  private loadCardDetails(cardId: string): void {
    this.isLoading = true;
    this.errorMessage = '';

    this.cardsService.getCardById(cardId).subscribe({
      next: (card) => {
        this.card = card;
        this.editTitle = card.title;
        this.editDescription = card.description ?? '';
        this.editDueDate = this.getInputDateValue(card.dueDateUtc);
        this.selectedTargetListId = card.listId;

        this.commentsService.getCommentsByCard(cardId).subscribe({
          next: (comments) => {
            this.comments = comments;
            this.isLoading = false;
          },
          error: () => {
            this.errorMessage = 'Failed to load comments.';
            this.isLoading = false;
          }
        });
      },
      error: () => {
        this.errorMessage = 'Failed to load card details.';
        this.isLoading = false;
      }
    });
  }

  startEditCard(): void {
    if (!this.card) return;

    this.editCardError = '';
    this.editTitle = this.card.title;
    this.editDescription = this.card.description ?? '';
    this.editDueDate = this.getInputDateValue(this.card.dueDateUtc);
    this.isEditingCard = true;
  }

  cancelEditCard(): void {
    if (!this.card) {
      this.isEditingCard = false;
      return;
    }

    this.editCardError = '';
    this.editTitle = this.card.title;
    this.editDescription = this.card.description ?? '';
    this.editDueDate = this.getInputDateValue(this.card.dueDateUtc);
    this.isEditingCard = false;
  }

  saveCard(): void {
    if (!this.card) return;

    const trimmedTitle = this.editTitle.trim();
    const trimmedDescription = this.editDescription.trim();

    this.editCardError = '';

    if (!trimmedTitle) {
      this.editCardError = 'Card title is required.';
      return;
    }

    if (this.editDueDate && this.editDueDate < this.minDate) {
      this.editCardError = 'Due date cannot be in the past.';
      return;
    }

    this.savingCard = true;

    this.cardsService.updateCard(this.card.id, {
      title: trimmedTitle,
      description: trimmedDescription || null,
      position: this.card.position,
      dueDateUtc: this.editDueDate ? new Date(this.editDueDate).toISOString() : null
    }).subscribe({
      next: () => {
        this.savingCard = false;
        this.isEditingCard = false;
        this.loadCardDetails(this.card!.id);
      },
      error: () => {
        this.editCardError = 'Failed to update card.';
        this.savingCard = false;
      }
    });
  }

  moveCard(): void {
    if (!this.card) {
      return;
    }

    this.moveCardError = '';

    if (!this.selectedTargetListId) {
      this.moveCardError = 'Please select a target column.';
      return;
    }

    if (this.selectedTargetListId === this.card.listId) {
      this.moveCardError = 'Card is already in this column.';
      return;
    }

    const target = this.moveTargets.find(t => t.id === this.selectedTargetListId);
    if (!target) {
      this.moveCardError = 'Target column was not found.';
      return;
    }

    this.movingCard = true;

    this.cardsService.moveCard(this.card.id, {
      targetListId: this.selectedTargetListId,
      targetPosition: target.cardCount + 1
    }).subscribe({
      next: () => {
        this.movingCard = false;
        this.loadBoardContext(this.boardId);
        this.loadCardDetails(this.card!.id);
      },
      error: (err) => {
        const backendMessage = err?.error?.message;
        this.moveCardError = backendMessage || 'Failed to move card.';
        this.movingCard = false;
      }
    });
  }

  assignUser(): void {
    if (!this.card) {
      return;
    }

    this.assignError = '';

    if (!this.selectedAssigneeId) {
      this.assignError = 'Please select a member.';
      return;
    }

    this.assigningUser = true;

    this.cardsService.assignUser(this.card.id, this.selectedAssigneeId).subscribe({
      next: () => {
        this.selectedAssigneeId = '';
        this.assigningUser = false;
        this.loadCardDetails(this.card!.id);
      },
      error: (err) => {
        const backendMessage = err?.error?.message;
        this.assignError = backendMessage || 'Failed to assign user.';
        this.assigningUser = false;
      }
    });
  }

  unassignUser(userId: string): void {
    if (!this.card) {
      return;
    }

    this.unassigningUserId = userId;

    this.cardsService.unassignUser(this.card.id, userId).subscribe({
      next: () => {
        this.unassigningUserId = null;
        this.loadCardDetails(this.card!.id);
      },
      error: () => {
        this.assignError = 'Failed to unassign user.';
        this.unassigningUserId = null;
      }
    });
  }

  deleteCard(): void {
    if (!this.card) return;

    const shouldDelete = window.confirm('Are you sure you want to delete this card?');
    if (!shouldDelete) return;

    this.deletingCard = true;

    this.cardsService.deleteCard(this.card.id).subscribe({
      next: () => {
        this.deletingCard = false;
        if (this.boardId) {
          this.router.navigate(['/boards', this.boardId]);
        }
      },
      error: () => {
        this.errorMessage = 'Failed to delete card.';
        this.deletingCard = false;
      }
    });
  }

  createComment(): void {
    const trimmedMessage = this.newCommentMessage.trim();

    this.createCommentError = '';

    if (!this.card) {
      this.createCommentError = 'Card not found.';
      return;
    }

    if (!this.currentBackendUser) {
      this.createCommentError = 'Current user is not available.';
      return;
    }

    if (!trimmedMessage) {
      this.createCommentError = 'Comment message is required.';
      return;
    }

    this.creatingComment = true;

    this.commentsService.createComment(this.card.id, {
      authorId: this.currentBackendUser.id,
      message: trimmedMessage
    }).subscribe({
      next: () => {
        this.newCommentMessage = '';
        this.creatingComment = false;
        this.loadCardDetails(this.card!.id);
      },
      error: () => {
        this.createCommentError = 'Failed to create comment.';
        this.creatingComment = false;
      }
    });
  }

  deleteComment(commentId: string): void {
    if (!this.card) return;

    const shouldDelete = window.confirm('Are you sure you want to delete this comment?');
    if (!shouldDelete) return;

    this.deletingCommentId = commentId;

    this.commentsService.deleteComment(commentId).subscribe({
      next: () => {
        this.deletingCommentId = null;
        this.loadCardDetails(this.card!.id);
      },
      error: () => {
        this.errorMessage = 'Failed to delete comment.';
        this.deletingCommentId = null;
      }
    });
  }
}