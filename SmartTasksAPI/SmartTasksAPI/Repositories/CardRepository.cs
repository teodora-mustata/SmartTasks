using Microsoft.EntityFrameworkCore;
using SmartTasksAPI.Models;
using SmartTasksAPI.Models.Data;

namespace SmartTasksAPI.Repositories
{

    public class CardRepository(ApplicationDbContext dbContext) : ICardRepository
    {
        public Task<List<CardItem>> GetByListIdAsync(Guid listId) => dbContext.Cards
            .Where(x => x.ListId == listId)
            .Include(x => x.Assignments)
                .ThenInclude(x => x.User)
            .OrderBy(x => x.Position)
            .AsNoTracking()
            .ToListAsync();

        public Task<CardItem?> GetByIdAsync(Guid cardId) => dbContext.Cards
            .Include(x => x.Assignments)
                .ThenInclude(x => x.User)
            .Include(x => x.Comments.OrderBy(c => c.CreatedAtUtc))
                .ThenInclude(x => x.Author)
            .FirstOrDefaultAsync(x => x.Id == cardId);

        public async Task<int> GetNextPositionAsync(Guid listId)
        {
            var maxPosition = await dbContext.Cards
                .Where(x => x.ListId == listId)
                .Select(x => (int?)x.Position)
                .MaxAsync();

            return (maxPosition ?? 0) + 1;
        }

        public async Task<CardItem> AddAsync(CardItem card)
        {
            dbContext.Cards.Add(card);
            await dbContext.SaveChangesAsync();
            return card;
        }

        public async Task UpdateAsync(CardItem card)
        {
            dbContext.Cards.Update(card);
            await dbContext.SaveChangesAsync();
        }

        public async Task DeleteAsync(CardItem card)
        {
            dbContext.Cards.Remove(card);
            await dbContext.SaveChangesAsync();
        }

        public Task<CardAssignment?> GetAssignmentAsync(Guid cardId, Guid userId) => dbContext.CardAssignments
            .FirstOrDefaultAsync(x => x.CardId == cardId && x.UserId == userId);

        public async Task AddAssignmentAsync(CardAssignment assignment)
        {
            dbContext.CardAssignments.Add(assignment);
            await dbContext.SaveChangesAsync();
        }

        public async Task RemoveAssignmentAsync(CardAssignment assignment)
        {
            dbContext.CardAssignments.Remove(assignment);
            await dbContext.SaveChangesAsync();
        }
    }

}
