using System.Collections.Generic;

namespace MIXUI.Entities
{
    public class Folder : Storable
    {
        public ICollection<Storable> Children { get; set; }
    }
}
