using System.ComponentModel.DataAnnotations;

namespace MIXUI.Dtos
{
    public class RegisterDto
    {
        [Required]
        public string Username { get; set; }
        [Required]
        [StringLength(100, MinimumLength = 5)]
        public string Password { get; set; }
    }
}
