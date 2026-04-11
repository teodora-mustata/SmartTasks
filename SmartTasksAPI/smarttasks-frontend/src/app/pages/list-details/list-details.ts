import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { BoardListModel } from '../../core/models/board-list.model';
import { CardItemModel } from '../../core/models/card-item.model';
import { Lists as ListsService } from '../../core/services/lists';
import { Cards as CardsService } from '../../core/services/cards';

@Component({
  selector: 'app-list-details',
  imports: [CommonModule, RouterLink, FormsModule],
  templateUrl: './list-details.html',
  styleUrl: './list-details.scss',
})
export class ListDetails implements OnInit {
  list: BoardListModel | null = null;
  cards: CardItemModel[] = [];
  isLoading = true;
  errorMessage = '';

  newCardTitle = '';
  newCardDescription = '';
  newCardDueDate = '';
  creatingCard = false;
  createCardError = '';

  readonly minDate = this.getTodayAsInputDate();

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private listsService: ListsService,
    private cardsService: CardsService
  ) { }

  ngOnInit(): void {
    const listId = this.route.snapshot.paramMap.get('listId');

    if (!listId) {
      this.errorMessage = 'List id is missing.';
      this.isLoading = false;
      return;
    }

    this.loadListDetails(listId);
  }

  private getTodayAsInputDate(): string {
    const now = new Date();
    const offset = now.getTimezoneOffset();
    return new Date(now.getTime() - offset * 60000).toISOString().split('T')[0];
  }

  getDueDateStatus(card: CardItemModel): 'none' | 'overdue' | 'today' | 'soon' | 'later' {
    if (!card.dueDateUtc) {
      return 'none';
    }

    const today = new Date();
    today.setHours(0, 0, 0, 0);

    const dueDate = new Date(card.dueDateUtc);
    dueDate.setHours(0, 0, 0, 0);

    const diffDays = Math.floor((dueDate.getTime() - today.getTime()) / (1000 * 60 * 60 * 24));

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

  private loadListDetails(listId: string): void {
    this.isLoading = true;
    this.errorMessage = '';

    this.listsService.getListById(listId).subscribe({
      next: (list) => {
        this.list = list;

        this.cardsService.getCardsByList(listId).subscribe({
          next: (cards) => {
            this.cards = this.sortCardsByDueDate(cards);
            this.isLoading = false;
          },
          error: () => {
            this.errorMessage = 'Failed to load cards for this list.';
            this.isLoading = false;
          }
        });
      },
      error: () => {
        this.errorMessage = 'Failed to load list details.';
        this.isLoading = false;
      }
    });
  }

  createCard(): void {
    const listId = this.list?.id;
    const trimmedTitle = this.newCardTitle.trim();
    const trimmedDescription = this.newCardDescription.trim();

    this.createCardError = '';

    if (!listId) {
      this.createCardError = 'List not found.';
      return;
    }

    if (!trimmedTitle) {
      this.createCardError = 'Card title is required.';
      return;
    }

    if (this.newCardDueDate && this.newCardDueDate < this.minDate) {
      this.createCardError = 'Due date cannot be in the past.';
      return;
    }

    this.creatingCard = true;

    this.cardsService.createCard(listId, {
      title: trimmedTitle,
      description: trimmedDescription || null,
      dueDateUtc: this.newCardDueDate ? new Date(this.newCardDueDate).toISOString() : null
    }).subscribe({
      next: () => {
        this.newCardTitle = '';
        this.newCardDescription = '';
        this.newCardDueDate = '';
        this.creatingCard = false;
        this.loadListDetails(listId);
      },
      error: () => {
        this.createCardError = 'Failed to create card.';
        this.creatingCard = false;
      }
    });
  }

  openCard(cardId: string): void {
    const boardId = this.route.snapshot.paramMap.get('boardId');
    const listId = this.list?.id;

    if (!boardId || !listId) {
      return;
    }

    this.router.navigate(['/boards', boardId, 'lists', listId, 'cards', cardId]);
  }
}