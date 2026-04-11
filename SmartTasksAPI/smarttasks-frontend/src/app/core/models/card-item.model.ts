import { CardCommentModel } from './card-comment.model';

export interface CardAssignmentUserModel {
    id: string;
    fullName: string;
    email: string;
}

export interface CardAssignmentModel {
    cardId: string;
    userId: string;
    assignedAtUtc: string;
    user?: CardAssignmentUserModel | null;
}

export interface CardItemModel {
    id: string;
    listId: string;
    title: string;
    description?: string | null;
    position: number;
    dueDateUtc?: string | null;
    createdAtUtc?: string;
    assignments?: CardAssignmentModel[];
    comments?: CardCommentModel[];
}