using SmartTasksAPI.Models;

namespace SmartTasksAPI.Services
{

    public interface ICommentService
    {
        Task<List<CardComment>> GetByCardIdAsync(Guid cardId);
        Task<CardComment> CreateAsync(Guid cardId, Guid authorId, string message);
        Task<bool> DeleteAsync(Guid commentId);
    }

}
