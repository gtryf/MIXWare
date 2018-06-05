using System.ComponentModel.DataAnnotations;

namespace MIXUI.Dtos
{
    public class PostUserDto
    {
        [Required]
        public string Username { get; set; }
        [Required]
        public string Password { get; set; }
    }
}
