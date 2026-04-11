import { BoardListModel } from './board-list.model';

export interface BoardMemberUserModel {
    id: string;
    fullName: string;
    email: string;
}

export interface BoardMemberModel {
    boardId: string;
    userId: string;
    role: number;
    addedAtUtc: string;
    user?: BoardMemberUserModel | null;
}

export interface BoardModel {
    id: string;
    name: string;
    description?: string | null;
    ownerId: string;
    createdAtUtc?: string;
    members?: BoardMemberModel[];
    lists?: BoardListModel[];
}