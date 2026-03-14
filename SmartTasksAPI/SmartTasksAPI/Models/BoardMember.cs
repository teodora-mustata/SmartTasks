using SmartTasksAPI.Models.Enums;

namespace SmartTasksAPI.Models
{
    public class BoardMember
    {
        public Guid BoardId { get; set; }
        public Guid UserId { get; set; }
        public BoardRole Role { get; set; } = BoardRole.Member;
        public DateTime AddedAtUtc { get; set; } = DateTime.UtcNow;

        public Board? Board { get; set; }
        public User? User { get; set; }
    }
}
