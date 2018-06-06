using Microsoft.AspNetCore.Identity;

namespace MIXUI.Entities
{
    public class AppUser : IdentityUser
    {
        public bool IsAdministrator { get; set; }
    }
}
