using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MIXUI.Dtos
{
    public class CreateSubmissionDto
    {
        [Required]
        public string FileId { get; set; }
        [Required]
        public string Type { get; set; }

        public bool ProduceListing { get; set; }
        public bool ProduceSymbolTable { get; set; }
        public string PrettyPrinter { get; set; }
    }

    public class SuccessfulSubmissionDto
    {
        public string Id { get; set; }
        public bool Successful { get; set; }
        public string AssemblyFileId { get; set; }
    }

    public class FailedSubmissionDto
    {
        public string Id { get; set; }
        public bool Successful { get; set; }
        public IEnumerable<ErrorInfoDto> Errors { get; set; }
        public IEnumerable<ErrorInfoDto> Warnings { get; set; }
    }

    public class ErrorInfoDto
    {
        public int Line { get; set; }
        public int Column { get; set; }
        public string Text { get; set; }
    }
}
