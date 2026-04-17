using SmartTasksAPI.Contracts.Auth;

namespace SmartTasksAPI.Services
{
    public interface IAuthService
    {
        Task<AuthResponse> RegisterAsync(string fullName, string email, string password);
        Task<AuthResponse> LoginAsync(string identifier, string password);
        Task<AuthUserResponse?> GetMeAsync(Guid userId);
    }
}
