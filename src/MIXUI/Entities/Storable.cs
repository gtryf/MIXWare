using System;
using System.ComponentModel.DataAnnotations;

namespace MIXUI.Entities
{
    public abstract class Storable
    {
        public string Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }
    }
}
