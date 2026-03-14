using SmartTasksAPI.Models;

namespace SmartTasksAPI.Repositories
{
    public interface IBoardRepository
    {
        Task<List<Board>> GetAllAsync();
        Task<Board?> GetByIdAsync(Guid boardId);
        Task<Board> AddAsync(Board board);
        Task UpdateAsync(Board board);
        Task DeleteAsync(Board board);
        Task<bool> MemberExistsAsync(Guid boardId, Guid userId);
        Task AddMemberAsync(BoardMember boardMember);
        Task RemoveMemberAsync(BoardMember boardMember);
        Task<BoardMember?> GetMemberAsync(Guid boardId, Guid userId);
    }

}
