namespace SmartTasksAPI.Models
{
    public class User
    {
        public Guid Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

        public ICollection<Board> OwnedBoards { get; set; } = new List<Board>();
        public ICollection<BoardMember> BoardMemberships { get; set; } = new List<BoardMember>();
        public ICollection<CardAssignment> CardAssignments { get; set; } = new List<CardAssignment>();
        public ICollection<CardComment> Comments { get; set; } = new List<CardComment>();
    }

}
