using System.ComponentModel.DataAnnotations;

namespace MIXUI.Dtos
{
    public class CreateStorableDto
    {
        [Required]
        public string Type { get; set; }
        [Required]
        [StringLength(100)]
        public string Name { get; set; }
        public string FileContents { get; set; }
    }
}
