using SmartTasksAPI.Models;

namespace SmartTasksAPI.Repositories
{
    public interface IListRepository
    {
        Task<List<BoardList>> GetByBoardIdAsync(Guid boardId);
        Task<BoardList?> GetByIdAsync(Guid listId);
        Task<int> GetNextPositionAsync(Guid boardId);
        Task<BoardList> AddAsync(BoardList list);
        Task UpdateAsync(BoardList list);
        Task DeleteAsync(BoardList list);
    }

}
