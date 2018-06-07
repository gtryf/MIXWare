using System;
using System.ComponentModel.DataAnnotations;

namespace MIXUI.Entities
{
    public abstract class Storable
    {
        public Guid Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }
    }
}
