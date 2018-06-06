using System.Collections.Generic;

namespace MIXUI.Dtos
{
    public class UserDto
    {
        public string Id { get; set; }
        public string Username { get; set; }
        public IEnumerable<WorkspaceDto> Workspaces { get; set; }
    }
}
