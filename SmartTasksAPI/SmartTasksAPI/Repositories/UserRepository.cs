using Microsoft.EntityFrameworkCore;
using SmartTasksAPI.Models;
using SmartTasksAPI.Models.Data;

namespace SmartTasksAPI.Repositories
{
    public class UserRepository(ApplicationDbContext dbContext) : IUserRepository
    {
        public Task<List<User>> GetAllAsync() => dbContext.Users.AsNoTracking().ToListAsync();

        public Task<User?> GetByIdAsync(Guid id) => dbContext.Users.FirstOrDefaultAsync(x => x.Id == id);

        public Task<User?> GetByEmailAsync(string email) => dbContext.Users.FirstOrDefaultAsync(x => x.Email == email);

        public async Task<User> AddAsync(User user)
        {
            dbContext.Users.Add(user);
            await dbContext.SaveChangesAsync();
            return user;
        }
    }

}
