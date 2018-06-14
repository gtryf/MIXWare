using System.ComponentModel.DataAnnotations;

namespace MIXUI.Dtos
{
    public class SubmissionDto
    {
        [Required]
        public string FileId { get; set; }
        [Required]
        public string Type { get; set; }

        public bool ProduceListing { get; set; }
        public bool ProduceSymbolTable { get; set; }
        public string PrettyPrinter { get; set; }
    }
}
