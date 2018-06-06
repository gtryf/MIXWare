using System;
using System.ComponentModel.DataAnnotations;

namespace MIXUI.Entities
{
    public class Workspace
    {
        public Guid Id { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 5)]
        public string Name { get; set; }

        public string IdentityId { get; set; }
        public AppUser Identity { get; set; }
    }
}
