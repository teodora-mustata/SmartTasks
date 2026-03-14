using System.ComponentModel.DataAnnotations;

namespace SmartTasksAPI.Contracts.Lists
{

    public class UpdateListRequest
    {
        [Required]
        [MinLength(2)]
        public string Name { get; set; } = string.Empty;

        [Range(1, int.MaxValue)]
        public int Position { get; set; }
    }

}
