using SmartTasksAPI.Models;

namespace SmartTasksAPI.Services
{

    public interface IUserService
    {
        Task<List<User>> GetAllAsync();
        Task<User?> GetByIdAsync(Guid id);
        Task<User> CreateAsync(string fullName, string email);
    }

}
