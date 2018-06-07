﻿using System.Collections.Generic;
using System.ComponentModel;
using Microsoft.AspNetCore.Identity;

namespace MIXUI.Entities
{
    public class AppUser : IdentityUser
    {
        public bool IsAdministrator { get; set; }

        public bool IsEnabled { get; set; }

        public virtual ICollection<Workspace> Workspaces { get; set; }
    }
}
