using SmartTasksAPI.Models;
using SmartTasksAPI.Repositories;

namespace SmartTasksAPI.Services
{

    public class ListService(IListRepository listRepository, IBoardRepository boardRepository) : IListService
    {
        public async Task<List<BoardList>> GetByBoardIdAsync(Guid boardId)
        {
            var board = await boardRepository.GetByIdAsync(boardId);
            if (board is null)
            {
                throw new KeyNotFoundException("Board not found.");
            }

            return await listRepository.GetByBoardIdAsync(boardId);
        }

        public Task<BoardList?> GetByIdAsync(Guid listId) => listRepository.GetByIdAsync(listId);

        public async Task<BoardList> CreateAsync(Guid boardId, string name)
        {
            var board = await boardRepository.GetByIdAsync(boardId);
            if (board is null)
            {
                throw new KeyNotFoundException("Board not found.");
            }

            var list = new BoardList
            {
                Id = Guid.NewGuid(),
                BoardId = boardId,
                Name = name.Trim(),
                Position = await listRepository.GetNextPositionAsync(boardId)
            };

            return await listRepository.AddAsync(list);
        }

        public async Task<bool> UpdateAsync(Guid listId, string name, int position)
        {
            var list = await listRepository.GetByIdAsync(listId);
            if (list is null)
            {
                return false;
            }

            list.Name = name.Trim();
            list.Position = position;
            await listRepository.UpdateAsync(list);
            return true;
        }

        public async Task<bool> DeleteAsync(Guid listId)
        {
            var list = await listRepository.GetByIdAsync(listId);
            if (list is null)
            {
                return false;
            }

            await listRepository.DeleteAsync(list);
            return true;
        }
    }

}
