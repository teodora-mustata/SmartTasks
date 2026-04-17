using SmartTasksAPI.Models;

namespace SmartTasksAPI.Services
{

    public interface IUserService
    {
        Task<List<User>> GetAllAsync();
    }

}
