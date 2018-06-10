using System.ComponentModel.DataAnnotations;

namespace MIXUI.Dtos
{
    public class CreateFileDto
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; }
        public string FileContents { get; set; }
    }
}
