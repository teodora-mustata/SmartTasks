using SmartTasksAPI.Models;
using SmartTasksAPI.Repositories;

namespace SmartTasksAPI.Services
{
    public class UserService(IUserRepository userRepository) : IUserService
    {
        public Task<List<User>> GetAllAsync() => userRepository.GetAllAsync();

        public Task<User?> GetByIdAsync(Guid id) => userRepository.GetByIdAsync(id);

        public async Task<User> CreateAsync(string fullName, string email)
        {
            var existing = await userRepository.GetByEmailAsync(email);
            if (existing is not null)
            {
                throw new InvalidOperationException("A user with this email already exists.");
            }

            var user = new User
            {
                Id = Guid.NewGuid(),
                FullName = fullName.Trim(),
                Email = email.Trim().ToLowerInvariant()
            };

            return await userRepository.AddAsync(user);
        }
    }

}
