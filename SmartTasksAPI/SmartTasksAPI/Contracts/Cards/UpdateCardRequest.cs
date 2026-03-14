using System.ComponentModel.DataAnnotations;

namespace SmartTasksAPI.Contracts.Cards
{

    public class UpdateCardRequest
    {
        [Required]
        [MinLength(2)]
        public string Title { get; set; } = string.Empty;

        public string? Description { get; set; }

        [Range(1, int.MaxValue)]
        public int Position { get; set; }

        public DateTime? DueDateUtc { get; set; }
    }

}
