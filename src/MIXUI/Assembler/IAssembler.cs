using MIXUI.Helpers;

namespace MIXUI.Assembler
{
    public interface IAssembler
    {
        IPrettyPrinter PrettyPrinter { get; set; }
        Union<AssemblySuccessResult, AssemblyErrorResult> Assemble(string sourceFileName, string text, bool produceSymbolTable, bool produceListing);
    }
}
