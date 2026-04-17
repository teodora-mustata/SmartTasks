using System.ComponentModel.DataAnnotations;

namespace SmartTasksAPI.Contracts.Auth
{
    public class LoginRequest
    {
        [Required]
        [MinLength(2)]
        public string Identifier { get; set; } = string.Empty;

        [Required]
        [MinLength(6)]
        public string Password { get; set; } = string.Empty;
    }
}
