using System.ComponentModel.DataAnnotations;

namespace SmartTasksAPI.Contracts.Comments
{

    public class CreateCommentRequest
    {
        [Required]
        public Guid AuthorId { get; set; }

        [Required]
        [MinLength(1)]
        public string Message { get; set; } = string.Empty;
    }

}
