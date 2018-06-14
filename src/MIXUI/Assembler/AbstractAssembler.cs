using System;
using System.IO;
using System.Linq;
using System.Text;
using MIXLib;
using MIXLib.Parser;
using MIXUI.Helpers;

namespace MIXUI.Assembler
{
    public abstract class AbstractAssembler : IAssembler
    {
        public IPrettyPrinter PrettyPrinter { get; set; }

        public abstract Union<AssemblySuccessResult, AssemblyErrorResult> Assemble(string sourceFileName, string text, bool produceSymbolTable, bool produceListing);

        protected string MakeListing(Parser parser, string text)
        {
            var sb = new StringBuilder();
            using (var writer = new StringWriter(sb))
            {
                writer.WriteLine(PrettyPrinter.Preamble);
                DoMakeListing(writer, parser, text);
                writer.WriteLine(PrettyPrinter.PostDocText);
            }
            return sb.ToString();
        }

        private void DoMakeListing(StringWriter writer, Parser parser, string text)
        {
            var reader = new StringReader(text);

            int line = 1;
            var al = parser.Assembly.ToList();
            while (reader.Peek() != -1)
            {
                string strLine = reader.ReadLine();
                MemoryCell cell = al.Find(c => c.SourceLocation == line);

                if (al.Exists(c => c.SourceLocation == line))
                    writer.WriteLine(PrettyPrinter.FormatInstruction(cell.Location, cell.Contents, line, strLine));
                else
                    writer.WriteLine(PrettyPrinter.FormatPseudo(line, strLine));

                line++;
            }

            DoMakeSymbols(writer, parser);
        }

        protected string MakeSymbolTable(Parser parser)
        {
            var sb = new StringBuilder();
            using (var writer = new StringWriter(sb))
            {
                writer.WriteLine(PrettyPrinter.Preamble);
                DoMakeSymbols(writer, parser);
                writer.WriteLine(PrettyPrinter.PostDocText);
            }
            return sb.ToString();
        }

        private void DoMakeSymbols(StringWriter writer, Parser parser)
        {
            var mainSymbs = from s in parser.SymbolTable
                            where !s.Key.StartsWith("=") && !s.Key.StartsWith("|")
                            select s;
            var localSymbs = from s in parser.SymbolTable
                             where s.Key.StartsWith("|")
                             select s;
            var lits = from s in parser.SymbolTable
                       where s.Key.StartsWith("=")
                       select s;

            writer.Write(PrettyPrinter.EmptyLine);
            writer.WriteLine(PrettyPrinter.FormatHeading("MAIN SYMBOLS"));
            foreach (var s in mainSymbs)
                writer.WriteLine(PrettyPrinter.FormatSymbol(s.Key, s.Value));

            writer.Write(PrettyPrinter.EmptyLine);
            writer.WriteLine(PrettyPrinter.FormatHeading("LOCAL SYMBOLS"));
            foreach (var s in localSymbs)
                writer.WriteLine(PrettyPrinter.FormatSymbol(s.Key, s.Value));

            writer.Write(PrettyPrinter.EmptyLine);
            writer.WriteLine(PrettyPrinter.FormatHeading("LITERALS"));
            foreach (var s in lits)
                writer.WriteLine(PrettyPrinter.FormatSymbol(s.Key, s.Value));
        }
    }
}
