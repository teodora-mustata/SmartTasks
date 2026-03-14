namespace SmartTasksAPI.Models
{
    public class BoardList
    {
        public Guid Id { get; set; }
        public Guid BoardId { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Position { get; set; }
        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

        public Board? Board { get; set; }
        public ICollection<CardItem> Cards { get; set; } = new List<CardItem>();
    }

}
