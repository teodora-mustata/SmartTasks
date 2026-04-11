import { CardItemModel } from './card-item.model';

export interface BoardListModel {
    id: string;
    boardId: string;
    name: string;
    position: number;
    createdAtUtc?: string;
    cards?: CardItemModel[];
}