using MIXLib;

namespace MIXUI.Assembler
{
    public interface IPrettyPrinter
    {
        string SourceFileName { get; set; }
        string Preamble { get; }
        string PostDocText { get; }
        string EmptyLine { get; }

        string FormatHeading(string headingText);
        string FormatInstruction(int location, MIXWord instruction, int lineNo, string line);
        string FormatPseudo(int lineNo, string line);
        string FormatSymbol(string name, MIXWord value);
    }
}
