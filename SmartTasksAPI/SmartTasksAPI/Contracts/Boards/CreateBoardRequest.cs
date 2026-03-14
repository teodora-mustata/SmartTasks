using System.ComponentModel.DataAnnotations;

namespace SmartTasksAPI.Contracts.Boards
{

    public class CreateBoardRequest
    {
        [Required]
        [MinLength(2)]
        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        [Required]
        public Guid OwnerId { get; set; }
    }

}
