export interface BoardModel {
    id: string;
    name: string;
    description?: string | null;
    ownerId?: string;
    createdAtUtc?: string;
}