using Microsoft.EntityFrameworkCore;
using SmartTasksAPI.Models;
using SmartTasksAPI.Models.Data;

namespace SmartTasksAPI.Repositories
{

    public class ListRepository(ApplicationDbContext dbContext) : IListRepository
    {
        public Task<List<BoardList>> GetByBoardIdAsync(Guid boardId) => dbContext.Lists
            .Where(x => x.BoardId == boardId)
            .OrderBy(x => x.Position)
            .AsNoTracking()
            .ToListAsync();

        public Task<BoardList?> GetByIdAsync(Guid listId) => dbContext.Lists
            .FirstOrDefaultAsync(x => x.Id == listId);

        public async Task<int> GetNextPositionAsync(Guid boardId)
        {
            var maxPosition = await dbContext.Lists
                .Where(x => x.BoardId == boardId)
                .Select(x => (int?)x.Position)
                .MaxAsync();

            return (maxPosition ?? 0) + 1;
        }

        public async Task<BoardList> AddAsync(BoardList list)
        {
            dbContext.Lists.Add(list);
            await dbContext.SaveChangesAsync();
            return list;
        }

        public async Task UpdateAsync(BoardList list)
        {
            dbContext.Lists.Update(list);
            await dbContext.SaveChangesAsync();
        }

        public async Task DeleteAsync(BoardList list)
        {
            dbContext.Lists.Remove(list);
            await dbContext.SaveChangesAsync();
        }
    }

}
