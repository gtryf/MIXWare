using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace MIXUI.Entities
{
    public class Submission
    {
        private string _errors;
        private string _warnings;

        public string Id { get; set; }
        public string IdentityId { get; set; }
        public virtual AppUser Identity { get; set; }

        public SubmissionStatus Status { get; set; }

        public bool Successful { get; set; }

        public int WordCount { get; set; }
        public string AssemblyFileId { get; set; }
        public string ListingFileId { get; set; }
        public string SymbolFileId { get; set; }

        [NotMapped]
        public IEnumerable<ErrorInfo> Errors
        {
            get => JsonConvert.DeserializeObject<IEnumerable<ErrorInfo>>(string.IsNullOrEmpty(_errors) ? "{}" : _errors);
            set => _errors = JsonConvert.SerializeObject(value);
        }

        [NotMapped]
        public IEnumerable<ErrorInfo> Warnings
        {
            get => JsonConvert.DeserializeObject<IEnumerable<ErrorInfo>>(string.IsNullOrEmpty(_warnings) ? "{}" : _warnings);
            set => _warnings = JsonConvert.SerializeObject(value);
        }
    }
}
