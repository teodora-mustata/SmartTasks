using SmartTasksAPI.Models;

namespace SmartTasksAPI.Repositories
{
    public interface ICommentRepository
    {
        Task<List<CardComment>> GetByCardIdAsync(Guid cardId);
        Task<CardComment?> GetByIdAsync(Guid commentId);
        Task<CardComment> AddAsync(CardComment comment);
        Task DeleteAsync(CardComment comment);
    }

}
