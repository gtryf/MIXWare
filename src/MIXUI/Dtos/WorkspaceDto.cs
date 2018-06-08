using System.Collections.Generic;

namespace MIXUI.Dtos
{
    public class WorkspaceDto
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public ICollection<StorableDto> Contents { get; set; }
    }
}