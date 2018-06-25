using System.ComponentModel.DataAnnotations;

namespace MIXUI.Dtos
{
    public class CreateWorkspaceDto
    {
        [Required]
        [StringLength(100, MinimumLength = 5)]
        public string Name { get; set; }

        [StringLength(2000)]
        public string Description { get; set; }
    }
}
