using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MIXUI.Entities
{
    public class Workspace
    {
        public string Id { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 5)]
        public string Name { get; set; }

        public string IdentityId { get; set; }
        public AppUser Identity { get; set; }

        public ICollection<Storable> Contents { get; set; }
    }
}
