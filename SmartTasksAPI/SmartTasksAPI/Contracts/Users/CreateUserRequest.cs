using System.ComponentModel.DataAnnotations;

namespace SmartTasksAPI.Contracts.Users
{

    public class CreateUserRequest
    {
        [Required]
        [MinLength(2)]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
    }

}
