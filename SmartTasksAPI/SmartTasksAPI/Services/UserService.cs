using SmartTasksAPI.Models;
using SmartTasksAPI.Repositories;

namespace SmartTasksAPI.Services
{
    public class UserService(IUserRepository userRepository) : IUserService
    {
        public Task<List<User>> GetAllAsync() => userRepository.GetAllAsync();

        
    }

}
