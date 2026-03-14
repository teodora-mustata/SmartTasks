namespace SmartTasksAPI.Models
{

    public class CardComment
    {
        public Guid Id { get; set; }
        public Guid CardId { get; set; }
        public Guid AuthorId { get; set; }
        public string Message { get; set; } = string.Empty;
        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

        public CardItem? Card { get; set; }
        public User? Author { get; set; }
    }

}
