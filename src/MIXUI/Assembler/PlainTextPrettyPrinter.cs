using System;
using MIXLib;

namespace MIXUI.Assembler
{
    public class PlainTextPrettyPrinter : IPrettyPrinter
    {
        public string SourceFileName { get; set; }
        public string Preamble => string.Empty;
        public string PostDocText => string.Empty;
        public string EmptyLine => Environment.NewLine;

        public string FormatHeading(string headingText) => "============== " + headingText + " ==============";

        public string FormatInstruction(int location, MIXWord instruction, int lineNo, string line)
        {
            string strLine = lineNo.ToString().PadLeft(4) + " " + line;
            string firstPart = string.Format("{0:0000}: {1} ", location, instruction.ToInstructionString());

            strLine = firstPart += strLine;

            return strLine;
        }

        public string FormatPseudo(int lineNo, string line)
            => new string(' ', 25) + lineNo.ToString().PadLeft(4) + " " + line;

        public string FormatSymbol(string name, MIXWord value)
            => name + "\t" + value + " = " + value.Value;
    }
}
