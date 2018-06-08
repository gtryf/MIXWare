using System.Collections.Generic;
using System.ComponentModel;
using Microsoft.AspNetCore.Identity;

namespace MIXUI.Entities
{
    public class AppUser : IdentityUser
    {
        public bool IsEnabled { get; set; }

        public virtual ICollection<Workspace> Workspaces { get; set; }
    }
}
