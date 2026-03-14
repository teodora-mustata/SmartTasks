using SmartTasksAPI.Models;

namespace SmartTasksAPI.Services
{

    public interface IListService
    {
        Task<List<BoardList>> GetByBoardIdAsync(Guid boardId);
        Task<BoardList?> GetByIdAsync(Guid listId);
        Task<BoardList> CreateAsync(Guid boardId, string name);
        Task<bool> UpdateAsync(Guid listId, string name, int position);
        Task<bool> DeleteAsync(Guid listId);
    }

}
