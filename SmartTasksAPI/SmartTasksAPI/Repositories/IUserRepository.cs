using SmartTasksAPI.Models;

namespace SmartTasksAPI.Repositories
{
    public interface IUserRepository
    {
        Task<List<User>> GetAllAsync();
        Task<User?> GetByIdAsync(Guid id);
        Task<User?> GetByEmailAsync(string email);
        Task<User> AddAsync(User user);
    }
}
