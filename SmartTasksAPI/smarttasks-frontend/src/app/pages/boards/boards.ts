import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { BoardModel } from '../../core/models/board.model';
import { Boards as BoardsService } from '../../core/services/boards';

@Component({
  selector: 'app-boards',
  imports: [CommonModule],
  templateUrl: './boards.html',
  styleUrl: './boards.scss',
})
export class Boards implements OnInit {
  boards: BoardModel[] = [];
  isLoading = true;
  errorMessage = '';

  constructor(private boardsService: BoardsService) { }

  ngOnInit(): void {
    this.loadBoards();
  }

  loadBoards(): void {
    this.isLoading = true;
    this.errorMessage = '';

    this.boardsService.getBoards().subscribe({
      next: (data) => {
        this.boards = data;
        this.isLoading = false;
      },
      error: () => {
        this.errorMessage = 'Failed to load boards.';
        this.isLoading = false;
      }
    });
  }
}