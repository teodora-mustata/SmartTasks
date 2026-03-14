using System.ComponentModel.DataAnnotations;

namespace SmartTasksAPI.Contracts.Cards
{
    public class MoveCardRequest
    {
        [Required]
        public Guid TargetListId { get; set; }

        [Range(1, int.MaxValue)]
        public int TargetPosition { get; set; }
    }

}
