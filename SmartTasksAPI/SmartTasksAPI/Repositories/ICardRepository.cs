using SmartTasksAPI.Models;

namespace SmartTasksAPI.Repositories
{
    public interface ICardRepository
    {
        Task<List<CardItem>> GetByListIdAsync(Guid listId);
        Task<CardItem?> GetByIdAsync(Guid cardId);
        Task<int> GetNextPositionAsync(Guid listId);
        Task<CardItem> AddAsync(CardItem card);
        Task UpdateAsync(CardItem card);
        Task DeleteAsync(CardItem card);
        Task<CardAssignment?> GetAssignmentAsync(Guid cardId, Guid userId);
        Task AddAssignmentAsync(CardAssignment assignment);
        Task RemoveAssignmentAsync(CardAssignment assignment);
    }

}
