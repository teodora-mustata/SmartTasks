namespace SmartTasksAPI.Contracts.Auth
{
    public class AuthUserResponse
    {
        public Guid Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public DateTime CreatedAtUtc { get; set; }
    }
}
