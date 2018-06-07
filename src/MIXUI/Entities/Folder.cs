using System.Collections.Generic;

namespace MIXUI.Entities
{
    public class Folder : Storable
    {
        public virtual ICollection<Storable> Children { get; set; }
    }
}
