namespace SmartTasksAPI.Contracts.Auth
{
    public class AuthResponse
    {
        public string Token { get; set; } = string.Empty;
        public AuthUserResponse User { get; set; } = new();
    }
}
