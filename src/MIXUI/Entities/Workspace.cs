using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MIXUI.Entities
{
    public class Workspace
    {
        public virtual string Id { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 5)]
        public virtual string Name { get; set; }

        public string IdentityId { get; set; }
        public virtual AppUser Identity { get; set; }

		public virtual ICollection<File> Files { get; set; }
    }
}
