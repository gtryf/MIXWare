using System.Collections.Generic;
using MIXLib;
using MIXUI.Helpers;

namespace MIXUI.Assembler
{
    public class BinaryAssembly
    {
        public MIXWord StartLoc { get; set; }
        public List<MemoryCell> Assembly { get; set; }
        public SerializableDictionary<string, MIXWord> SymbolTable { get; set; }
    }
}
