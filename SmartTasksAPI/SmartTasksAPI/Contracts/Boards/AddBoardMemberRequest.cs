using System.ComponentModel.DataAnnotations;

namespace SmartTasksAPI.Contracts.Boards
{
    public class AddBoardMemberRequest
    {
        [Required]
        public Guid UserId { get; set; }
    }
}
