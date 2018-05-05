using System;

namespace MIXLib
{
    [Serializable]
    public struct MemoryCell
    {
        public int SourceLocation { get; set; }
        public int Location { get; set; }
        public MIXWord Contents { get; set; }
    }
}