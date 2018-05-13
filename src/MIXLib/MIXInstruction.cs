using System;
using System.Collections.Generic;

namespace MIXLib
{
    public class MIXInstruction
    {
        private Action<MIXWord, byte, byte> executionProc;
        public string Name { get; private set; }

        public MIXInstruction(string name, Action<MIXWord, byte, byte> executionProc)
        {
            this.Name = name;
            this.executionProc = executionProc;
        }

        public void Execute(MIXWord address, byte index, byte field)
		    => executionProc(address, index, field);

        public void Execute(MIXWord address, byte index, byte left, byte right)
		    => executionProc(address, index, (byte)(left * 8 + right));
    }
}