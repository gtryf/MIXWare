using System;
using System.ComponentModel.DataAnnotations;

namespace MIXUI.Entities
{
    public abstract class Storable
    {
        public string Id { get; set; }

        [Required]
        [StringLength(100)]
        public virtual string Name { get; set; }

        public virtual string ParentId { get; set; }
        public virtual Storable Parent { get; set; }
    }
}
