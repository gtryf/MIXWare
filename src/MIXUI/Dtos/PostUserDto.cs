using System.ComponentModel.DataAnnotations;

namespace MIXUI.Dtos
{
    /// <summary>
    /// An existing user inside the database
    /// </summary>
    public class PostUserDto
    {
        /// <summary>
        /// The user's external user name
        /// </summary>
        [Required]
        public string Username { get; set; }
        /// <summary>
        /// The user's password
        /// </summary>
        [Required]
        public string Password { get; set; }
    }
}
