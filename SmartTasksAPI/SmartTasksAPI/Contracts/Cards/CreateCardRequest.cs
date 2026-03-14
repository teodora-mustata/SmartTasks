using System.ComponentModel.DataAnnotations;

namespace SmartTasksAPI.Contracts.Cards
{

    public class CreateCardRequest
    {
        [Required]
        [MinLength(2)]
        public string Title { get; set; } = string.Empty;

        public string? Description { get; set; }
        public DateTime? DueDateUtc { get; set; }
    }

}
