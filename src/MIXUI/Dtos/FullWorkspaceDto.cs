using System.Collections.Generic;

namespace MIXUI.Dtos
{
    public class FullWorkspaceDto : ShortWorkspaceDto
    {
        public IEnumerable<FileDto> Files { get; set; }
    }
}