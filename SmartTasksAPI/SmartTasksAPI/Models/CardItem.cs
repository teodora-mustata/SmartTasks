namespace SmartTasksAPI.Models
{

    public class CardItem
    {
        public Guid Id { get; set; }
        public Guid ListId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int Position { get; set; }
        public DateTime? DueDateUtc { get; set; }
        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

        public BoardList? List { get; set; }
        public ICollection<CardAssignment> Assignments { get; set; } = new List<CardAssignment>();
        public ICollection<CardComment> Comments { get; set; } = new List<CardComment>();
    }
}
