using System.Collections.Generic;
using MIXLib.Parser;

namespace MIXUI.Assembler
{
    public class AssemblySuccessResult
    {
        public int WordCount { get; }
        public byte[] Assembly { get; }
        public string Listing { get; set; }
        public string SymbolTable { get; set; }
        public AssemblySuccessResult(int wordCount, byte[] assembly)
        {
            this.WordCount = wordCount;
            this.Assembly = assembly;
        }
    }

    public class AssemblyErrorResult
    {
        public IEnumerable<ErrorInfo> Errors { get; }
        public IEnumerable<ErrorInfo> Warnings { get; }
        public AssemblyErrorResult(IEnumerable<ErrorInfo> errors, IEnumerable<ErrorInfo> warnings)
        {
            this.Errors = errors;
            this.Warnings = warnings;
        }
    }
}
