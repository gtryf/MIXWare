using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MIXUI.Entities
{
    public abstract class EntityBase
    {
        [Required]
        public DateTime CreatedUtc { get; set; }
        
        [Required]
        public DateTime UpdatedUtc { get; set; }
    }
}