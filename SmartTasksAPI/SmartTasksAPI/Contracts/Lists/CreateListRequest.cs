using System.ComponentModel.DataAnnotations;

namespace SmartTasksAPI.Contracts.Lists
{
    public class CreateListRequest
    {
        [Required]
        [MinLength(2)]
        public string Name { get; set; } = string.Empty;
    }

}
