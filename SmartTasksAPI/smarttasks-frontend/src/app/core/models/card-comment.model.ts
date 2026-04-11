export interface CardCommentAuthorModel {
    id: string;
    fullName: string;
    email: string;
}

export interface CardCommentModel {
    id: string;
    cardId: string;
    authorId: string;
    message: string;
    createdAtUtc: string;
    author?: CardCommentAuthorModel | null;
}