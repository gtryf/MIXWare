using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MIXUI.Entities
{
    public class File
    {
		public string Id { get; set; }

        [Required]
        [StringLength(100)]
        public virtual string Name { get; set; }

        [Required]
        public virtual byte[] Data { get; set; }

        [Required]
		[DefaultValue(FileType.Source)]
		public FileType Type { get; set; }

		public string WorkspaceId { get; set; }
		public Workspace Workspace { get; set; }
    }
}
