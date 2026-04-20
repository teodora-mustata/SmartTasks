import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { forkJoin } from 'rxjs';

import { BoardModel, BoardMemberModel } from '../../core/models/board.model';
import { BoardListModel } from '../../core/models/board-list.model';
import { CardItemModel, CardAssignmentModel } from '../../core/models/card-item.model';

import { Boards as BoardsService } from '../../core/services/boards';
import { Cards as CardsService } from '../../core/services/cards';
import { Lists as ListsService } from '../../core/services/lists';
import { Users as UsersService, BackendUser } from '../../core/services/users';
import { AuthUser } from '../../core/services/auth';
import { BoardAccess } from '../../core/services/board-access';

interface KanbanColumn {
  list: BoardListModel;
  cards: CardItemModel[];
}

@Component({
  selector: 'app-board-details',
  imports: [CommonModule, RouterLink, FormsModule],
  templateUrl: './board-details.html',
  styleUrl: './board-details.scss',
})
export class BoardDetails implements OnInit {
  board: BoardModel | null = null;
  columns: KanbanColumn[] = [];
  isLoading = true;
  errorMessage = '';

  currentBackendUser: AuthUser | null = null;

  allUsers: BackendUser[] = [];
  selectedMemberId = '';
  addingMember = false;
  memberError = '';
  removingMemberId: string | null = null;

  readonly systemLists = [
    'Backlog',
    'Ready',
    'In progress',
    'In review',
    'Done'
  ];

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private boardsService: BoardsService,
    private cardsService: CardsService,
    private listsService: ListsService,
    private usersService: UsersService,
    private boardAccess: BoardAccess
  ) { }

  ngOnInit(): void {
    const boardId = this.route.snapshot.paramMap.get('id');

    if (!boardId) {
      this.errorMessage = 'Board id is missing.';
      this.isLoading = false;
      return;
    }

    this.loadUsers();

    this.boardAccess.getCurrentBackendUser().subscribe({
      next: (user) => {
        this.currentBackendUser = user;

        if (!user) {
          this.router.navigate(['/boards']);
          return;
        }

        this.loadBoard(boardId);
      },
      error: () => {
        this.errorMessage = 'Failed to resolve current user.';
        this.isLoading = false;
      }
    });
  }

  get availableUsers(): BackendUser[] {
    const memberIds = new Set((this.board?.members ?? []).map(m => m.userId));
    return this.allUsers.filter(user => !memberIds.has(user.id));
  }

  getColumnDescription(name: string): string {
    switch (name) {
      case 'Backlog':
        return 'This item has not been started';
      case 'Ready':
        return 'This is ready to be picked up';
      case 'In progress':
        return 'This is actively being worked on';
      case 'In review':
        return 'This item is in review';
      case 'Done':
        return 'This has been completed';
      default:
        return '';
    }
  }

  getMemberName(member: BoardMemberModel): string {
    return member.user?.fullName || 'Unknown user';
  }

  getMemberEmail(member: BoardMemberModel): string {
    return member.user?.email || '';
  }

  isOwnerMember(member: BoardMemberModel): boolean {
    return !!this.board && member.userId === this.board.ownerId;
  }

  getCardAssigneeNames(card: CardItemModel): string[] {
    const assignments = card.assignments ?? [];
    const members = this.board?.members ?? [];

    return assignments.map((assignment: CardAssignmentModel) => {
      const fromAssignment = assignment.user?.fullName;
      if (fromAssignment) {
        return fromAssignment;
      }

      const matchingMember = members.find(member => member.userId === assignment.userId);
      return matchingMember?.user?.fullName || 'Unknown user';
    });
  }

  getVisibleAssigneeNames(card: CardItemModel): string[] {
    return this.getCardAssigneeNames(card).slice(0, 2);
  }

  getRemainingAssigneeCount(card: CardItemModel): number {
    const total = this.getCardAssigneeNames(card).length;
    return total > 2 ? total - 2 : 0;
  }

  hasAssignees(card: CardItemModel): boolean {
    return (card.assignments?.length ?? 0) > 0;
  }

  private loadUsers(): void {
    this.usersService.getUsers().subscribe({
      next: (users) => {
        this.allUsers = users;
      }
    });
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

  private sortCardsByDueDate(cards: CardItemModel[]): CardItemModel[] {
    return [...cards].sort((a, b) => {
      const aDue = a.dueDateUtc ? new Date(a.dueDateUtc).getTime() : Number.MAX_SAFE_INTEGER;
      const bDue = b.dueDateUtc ? new Date(b.dueDateUtc).getTime() : Number.MAX_SAFE_INTEGER;

      if (aDue !== bDue) {
        return aDue - bDue;
      }

      return a.position - b.position;
    });
  }

  private loadBoard(boardId: string): void {
    this.isLoading = true;
    this.errorMessage = '';

    this.boardsService.getBoardById(boardId).subscribe({
      next: (board) => {
        if (!this.boardAccess.canAccessBoard(board, this.currentBackendUser?.id)) {
          this.router.navigate(['/boards']);
          return;
        }

        this.board = board;

        const existingLists = board.lists ?? [];
        const existingNames = existingLists.map(l => l.name.trim().toLowerCase());

        const missingLists = this.systemLists.filter(
          name => !existingNames.includes(name.trim().toLowerCase())
        );

        if (missingLists.length > 0) {
          forkJoin(
            missingLists.map(name => this.listsService.createList(board.id, name))
          ).subscribe({
            next: () => this.loadBoard(boardId),
            error: () => {
              this.errorMessage = 'Failed to create the default columns.';
              this.isLoading = false;
            }
          });
          return;
        }

        const orderedLists: BoardListModel[] = this.systemLists
          .map(systemName =>
            existingLists.find(
              l => l.name.trim().toLowerCase() === systemName.trim().toLowerCase()
            )!
          )
          .filter(Boolean);

        const requests = orderedLists.map(list => this.cardsService.getCardsByList(list.id));

        forkJoin(requests).subscribe({
          next: (cardsByList) => {
            this.columns = orderedLists.map((list, index) => ({
              list,
              cards: this.sortCardsByDueDate(cardsByList[index] ?? [])
            }));

            this.isLoading = false;
          },
          error: () => {
            this.errorMessage = 'Failed to load cards for this board.';
            this.isLoading = false;
          }
        });
      },
      error: () => {
        this.errorMessage = 'Failed to load board details.';
        this.isLoading = false;
      }
    });
  }

  addMember(): void {
    if (!this.board) {
      return;
    }

    this.memberError = '';

    if (!this.selectedMemberId) {
      this.memberError = 'Please select a user.';
      return;
    }

    this.addingMember = true;

    this.boardsService.addMember(this.board.id, {
      userId: this.selectedMemberId
    }).subscribe({
      next: () => {
        this.selectedMemberId = '';
        this.addingMember = false;
        this.loadBoard(this.board!.id);
      },
      error: () => {
        this.memberError = 'Failed to add member.';
        this.addingMember = false;
      }
    });
  }

  removeMember(userId: string): void {
    if (!this.board) {
      return;
    }

    const shouldRemove = window.confirm('Are you sure you want to remove this member from the board?');
    if (!shouldRemove) {
      return;
    }

    this.removingMemberId = userId;

    this.boardsService.removeMember(this.board.id, userId).subscribe({
      next: () => {
        this.removingMemberId = null;
        this.loadBoard(this.board!.id);
      },
      error: () => {
        this.memberError = 'Failed to remove member.';
        this.removingMemberId = null;
      }
    });
  }

  openCard(cardId: string): void {
    if (!this.board?.id) {
      return;
    }

    this.router.navigate(['/boards', this.board.id, 'cards', cardId]);
  }

  goToCreateCard(): void {
    if (!this.board?.id) {
      return;
    }

    this.router.navigate(['/boards', this.board.id, 'cards', 'new']);
  }
}