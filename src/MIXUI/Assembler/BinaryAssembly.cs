using System.Collections.Generic;
using MIXLib;

namespace MIXUI.Assembler
{
    public class BinaryAssembly
    {
        public MIXWord StartLoc { get; set; }
        public IEnumerable<MemoryCell> Assembly { get; set; }
        public Dictionary<string, MIXWord> SymbolTable { get; set; }
    }
}
