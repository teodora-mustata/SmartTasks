using SmartTasksAPI.Models;

namespace SmartTasksAPI.Services
{

    public interface ICardService
    {
        Task<List<CardItem>> GetByListIdAsync(Guid listId);
        Task<CardItem?> GetByIdAsync(Guid cardId);
        Task<CardItem> CreateAsync(Guid listId, string title, string? description, DateTime? dueDateUtc);
        Task<bool> UpdateAsync(Guid cardId, string title, string? description, int position, DateTime? dueDateUtc);
        Task<bool> MoveAsync(Guid cardId, Guid targetListId, int targetPosition);
        Task<bool> DeleteAsync(Guid cardId);
        Task<bool> AssignUserAsync(Guid cardId, Guid userId);
        Task<bool> UnassignUserAsync(Guid cardId, Guid userId);
    }

}
