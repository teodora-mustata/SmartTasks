using Microsoft.EntityFrameworkCore;
using SmartTasksAPI.Models;
using SmartTasksAPI.Models.Data;

namespace SmartTasksAPI.Repositories
{

    public class BoardRepository(ApplicationDbContext dbContext) : IBoardRepository
    {
        public Task<List<Board>> GetAllAsync() => dbContext.Boards
            .Include(x => x.Owner)
            .Include(x => x.Members)
                .ThenInclude(x => x.User)
            .Include(x => x.Lists)
            .AsNoTracking()
            .ToListAsync();

        public Task<Board?> GetByIdAsync(Guid boardId) => dbContext.Boards
            .Include(x => x.Owner)
            .Include(x => x.Members)
                .ThenInclude(x => x.User)
            .Include(x => x.Lists.OrderBy(l => l.Position))
            .FirstOrDefaultAsync(x => x.Id == boardId);

        public async Task<Board> AddAsync(Board board)
        {
            dbContext.Boards.Add(board);
            await dbContext.SaveChangesAsync();
            return board;
        }

        public async Task UpdateAsync(Board board)
        {
            dbContext.Boards.Update(board);
            await dbContext.SaveChangesAsync();
        }

        public async Task DeleteAsync(Board board)
        {
            dbContext.Boards.Remove(board);
            await dbContext.SaveChangesAsync();
        }

        public Task<bool> MemberExistsAsync(Guid boardId, Guid userId) => dbContext.BoardMembers
            .AnyAsync(x => x.BoardId == boardId && x.UserId == userId);

        public async Task AddMemberAsync(BoardMember boardMember)
        {
            dbContext.BoardMembers.Add(boardMember);
            await dbContext.SaveChangesAsync();
        }

        public Task<BoardMember?> GetMemberAsync(Guid boardId, Guid userId) => dbContext.BoardMembers
            .FirstOrDefaultAsync(x => x.BoardId == boardId && x.UserId == userId);

        public async Task RemoveMemberAsync(BoardMember boardMember)
        {
            dbContext.BoardMembers.Remove(boardMember);
            await dbContext.SaveChangesAsync();
        }
    }
}
