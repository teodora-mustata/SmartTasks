using Microsoft.EntityFrameworkCore;
using SmartTasksAPI.Models;
using SmartTasksAPI.Models.Data;

namespace SmartTasksAPI.Repositories
{

    public class CommentRepository(ApplicationDbContext dbContext) : ICommentRepository
    {
        public Task<List<CardComment>> GetByCardIdAsync(Guid cardId) => dbContext.CardComments
            .Where(x => x.CardId == cardId)
            .Include(x => x.Author)
            .OrderBy(x => x.CreatedAtUtc)
            .AsNoTracking()
            .ToListAsync();

        public Task<CardComment?> GetByIdAsync(Guid commentId) => dbContext.CardComments
            .FirstOrDefaultAsync(x => x.Id == commentId);

        public async Task<CardComment> AddAsync(CardComment comment)
        {
            dbContext.CardComments.Add(comment);
            await dbContext.SaveChangesAsync();
            return comment;
        }

        public async Task DeleteAsync(CardComment comment)
        {
            dbContext.CardComments.Remove(comment);
            await dbContext.SaveChangesAsync();
        }
    }

}
