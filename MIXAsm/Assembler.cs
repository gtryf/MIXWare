using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;

using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

using MIXLib;
using MIXLib.Util;
using MIXLib.Parser;

namespace MIXAsm
{
    public enum Format
    {
        Card,
        Binary
    }

    public class MIXAssembler
    {
        public string InputFile { get; private set; }
        public string OutputFile { get; private set; }
        public Format OutputFormat { get; set; }
        public IPrettyPrinter PrettyPrinter { get; set; }
        private readonly Parser parser;

        public string ListingFile { get; set; }
        public bool MakeSymbolTable { get; set; }

        private string symbTabFile;
        public string SymbolTableFile
        {
            get
            {
                return symbTabFile;
            }
            set
            {
                if (!string.IsNullOrEmpty(value))
                    MakeSymbolTable = true;
                symbTabFile = value;
            }
        }

        public MIXAssembler(string inputFile, string outputFile, Format outputFormat)
        {
            InputFile = inputFile;
            OutputFormat = outputFormat;
            ListingFile = null;
            MakeSymbolTable = false;
            SymbolTableFile = null;
            OutputFile = outputFile;
            PrettyPrinter = new PlainTextPrinter();

            TextReader reader = new StreamReader(InputFile);
            this.parser = new Parser(reader);
        }

        public MIXAssembler(string inputFile, string outputFile) : this(inputFile, outputFile, Format.Binary) { }

        public MIXAssembler(string inputFile, Format outputFormat) : this(inputFile, null, outputFormat) { }

        public MIXAssembler(string inputFile) : this(inputFile, null, Format.Binary) { }

        public int Assemble()
        {
            parser.ParseProgram();

            if (Errors.Count() == 0)
            {
                if (OutputFormat == Format.Binary)
                    AssembleBinary();
                else
                    AssembleCards();

                return parser.Assembly.Count();
            }
            else
                return 0;
        }

        public IEnumerable<ErrorInfo> Errors { get { return parser.Errors; } }
        public IEnumerable<ErrorInfo> Warnings { get { return parser.Warnings; } }

        private void AssembleCards()
        {
            StreamWriter writer;
            if (string.IsNullOrEmpty(OutputFile))
                writer = new StreamWriter(Console.OpenStandardOutput(), System.Text.Encoding.UTF8);
            else
                writer = new StreamWriter(OutputFile, false, System.Text.Encoding.UTF8);

            // Write out the loading routine
            writer.WriteLine(" O O6 Z O6    I C O4 0 EH A  F F CF 0  E   EU 0 IH G BB   EJ  CA. Z EU   EH E BA");
            writer.WriteLine("   EU 2A-H S BB  C U 1AEH 2AEN V  E  CLU  ABG Z EH E BB J B. A  9               ");

            byte remainder = (byte)(parser.Assembly.Count() % 7);
            string comment = Path.GetFileNameWithoutExtension(InputFile).PadRight(5).Substring(0, 5).ToUpper();

            string outp = "";

            var interim = parser.Assembly.OrderBy(x => x.Location)
                .Select((x, i) => new { Index = i, Value = x })
                .GroupBy(x => x.Index - x.Value.Location)
                .Select(x => x.Select((v, i) => new { Index = i, Value = v.Value })
                .GroupBy(l => l.Index / 7)
                .Select(k => k.Select(v => v.Value)).ToList()
                .ToList())
                .ToList();

            List<List<MemoryCell>> chunks = new List<List<MemoryCell>>();
            foreach (var i in interim)
                foreach (var i2 in i)
                    chunks.Add(i2.ToList());

            foreach (var chunk in chunks)
            {
                outp = comment + chunk.Count + string.Format("{0:0000}", chunk.First().Location);
                foreach (var cell in chunk)
                {
                    int v = cell.Contents.Value;

                    if (v < 0)
                    {
                        outp += Math.Abs(v).ToString().PadLeft(10, '0');
                        // Replace the last digit with the appropriate code
                        byte c = MIXMachine.CHAR_TABLE[outp[outp.Length - 1]];
                        outp = outp.Substring(0, outp.Length - 1);
                        outp += MIXMachine.CHAR_TABLE.Where(entry => entry.Value == c - 20).First().Key.ToString(); ;
                    }
                    else
                        outp += v.ToString().PadLeft(10, '0');
                }
                writer.WriteLine(outp.PadRight(80));
            }

            // Transfer card
            writer.WriteLine(("TRANS0" + parser.StartLoc.Value.ToString().PadLeft(4, '0')).PadRight(80));

            writer.Flush();
            writer.Close();
        }

        private void AssembleBinary()
        {
            IFormatter formatter = new BinaryFormatter();
            Stream stream;
            if (string.IsNullOrEmpty(OutputFile))
                stream = Console.OpenStandardOutput();
            else
                stream = new FileStream(OutputFile, FileMode.Create, FileAccess.Write, FileShare.None);

            formatter.Serialize(stream, parser.StartLoc);
            formatter.Serialize(stream, parser.Assembly);
            formatter.Serialize(stream, parser.SymbolTable);
            stream.Close();
        }

        public void MakeOther()
        {
            if (this.MakeSymbolTable)
            {
                StreamWriter writer;
                if (this.SymbolTableFile == null)
                    writer = new StreamWriter(Console.OpenStandardOutput());
                else
                    writer = new StreamWriter(this.SymbolTableFile, false, System.Text.Encoding.UTF8);

                writer.WriteLine(PrettyPrinter.Preamble);
                MakeSymbols(writer);
                writer.WriteLine(PrettyPrinter.PostDocText);

                writer.Flush();
                writer.Close();
            }

            if (!string.IsNullOrEmpty(this.ListingFile))
            {
                StreamWriter writer = new StreamWriter(this.ListingFile, false, System.Text.Encoding.UTF8);

                writer.WriteLine(PrettyPrinter.Preamble);
                MakeListing(writer);
                writer.WriteLine(PrettyPrinter.PostDocText);

                writer.Flush();
                writer.Close();
            }
        }

        private void MakeListing(StreamWriter writer)
        {
            StreamReader reader = new StreamReader(InputFile);

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

            MakeSymbols(writer);
        }

        private void MakeSymbols(StreamWriter writer)
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