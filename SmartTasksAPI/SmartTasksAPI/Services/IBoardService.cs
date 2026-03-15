using SmartTasksAPI.Models;

namespace SmartTasksAPI.Services
{
    public interface IBoardService
    {
        Task<List<Board>> GetAllAsync();
        Task<Board?> GetByIdAsync(Guid boardId);
        Task<Board> CreateAsync(string name, string? description, Guid ownerId);
        Task<bool> UpdateAsync(Guid boardId, string name, string? description);
        Task<bool> DeleteAsync(Guid boardId);
        Task<bool> AddMemberAsync(Guid boardId, Guid userId);
        Task<bool> RemoveMemberAsync(Guid boardId, Guid userId);
    }

}
