using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace MIXUI.Dtos
{
    public class CreateWorkspaceDto
    {
        [Required]
        [StringLength(100, MinimumLength = 5)]
        public string Name { get; set; }

        [StringLength(2000)]
        public string Description { get; set; }
    }

    public class UpdateWorkspaceDto
    {
        [StringLength(100, MinimumLength = 5)]
        public string Name { get; set; }

        [StringLength(2000)]
        public string Description { get; set; }
    }

    public class FullWorkspaceDto : ShortWorkspaceDto
    {
        public IEnumerable<FileDto> Files { get; set; }
    }

    public class ShortWorkspaceDto
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int FileCount { get; set; }
    }
}
