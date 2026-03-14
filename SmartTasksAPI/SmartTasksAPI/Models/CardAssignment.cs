namespace SmartTasksAPI.Models
{
    public class CardAssignment
    {
        public Guid CardId { get; set; }
        public Guid UserId { get; set; }
        public DateTime AssignedAtUtc { get; set; } = DateTime.UtcNow;

        public CardItem? Card { get; set; }
        public User? User { get; set; }
    }

}
