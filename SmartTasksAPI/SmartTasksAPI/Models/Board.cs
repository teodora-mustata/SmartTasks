namespace SmartTasksAPI.Models
{

    public class Board
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public Guid OwnerId { get; set; }
        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

        public User? Owner { get; set; }
        public ICollection<BoardMember> Members { get; set; } = new List<BoardMember>();
        public ICollection<BoardList> Lists { get; set; } = new List<BoardList>();
    }

}
