using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

using MIXLib;
using MIXLib.Util;
using MIXLib.Parser;

namespace MIXAsm
{
    class Program
    {
        private static string GetInputFile(Dictionary<string, string> cmdLine)
        {
            var a = cmdLine.Keys.Where(cl => !cl.StartsWith("-") && !cl.StartsWith("/"));
            if (a.Count() > 0)
                return a.First();

            return null;
        }

        private static string GetOutputFile(Dictionary<string, string> cmdLine)
        {
            if (cmdLine.ContainsKey("--output"))
                return cmdLine["--output"];

            return null;
        }

        static void Main(string[] args)
        {
            Dictionary<string, string> aliases = new Dictionary<string, string>();
            aliases.Add("-s", "--symtab");
            aliases.Add("-o", "--output");
            aliases.Add("-lf", "--list-file");
            aliases.Add("-f", "--format");
            aliases.Add("-a", "--append-deck");
            aliases.Add("-pp", "--pretty-print");
            aliases.Add("-?", "--help");
            aliases.Add("-h", "--help");

            Dictionary<string, string> cmdLine = CommandLineHelper.SplitCommandLine(Environment.CommandLine, aliases, true);

            Console.Error.WriteLine("MIXAL Assembler, (c) 2009 George Tryfonas");

            string inFile = GetInputFile(cmdLine);
            if (string.IsNullOrEmpty(inFile))
            {
                Console.Error.WriteLine("No input file specified.");
                Environment.Exit(-1);
            }

            string outFile = GetOutputFile(cmdLine);

            try
            {
                MIXAssembler assembler = new MIXAssembler(inFile, outFile);

                if (cmdLine.ContainsKey("--format"))
                {
                    switch (cmdLine["--format"])
                    {
                        case "binary":
                            assembler = new MIXAssembler(inFile, outFile, Format.Binary);
                            break;
                        case "card":
                            assembler = new MIXAssembler(inFile, outFile, Format.Card);
                            break;
                        default:
                            Console.Error.WriteLine(string.Format("Unknown output format: '{0}'", cmdLine["--format"]));
                            System.Environment.Exit(-1);
                            break;
                    }
                }
                else
                    assembler = new MIXAssembler(inFile, outFile);

                if (cmdLine.ContainsKey("--symtab"))
                {
                    assembler.MakeSymbolTable = true;
                    assembler.SymbolTableFile = cmdLine["--symtab"];
                }

                if (cmdLine.ContainsKey("--list-file"))
                    assembler.ListingFile = cmdLine["--list-file"];

                Console.Error.WriteLine(string.Format("Output file is: '{0}'\nOutput format is: {1}", 
                    string.IsNullOrEmpty(assembler.OutputFile) ? "CONSOLE" : assembler.OutputFile, assembler.OutputFormat));

                int wordCount = assembler.Assemble();
                if (assembler.Errors.Count() > 0)
                {
                    foreach (var e in assembler.Errors)
                        Console.Error.WriteLine(string.Format("Error at {0},{1}: {2}", e.Line, e.Column, e.Text));
                    Console.Error.WriteLine("Assembly did not complete due to errors.");
                }
                else
                {
                    Console.Error.WriteLine(string.Format("{0} word(s) assembled.", wordCount));
                    if (assembler.Warnings.Count() > 0)
                    {
                        foreach (var e in assembler.Warnings)
                            Console.Error.WriteLine(string.Format("Error at {0},{1}: {2}", e.Line, e.Column, e.Text));
                        Console.Error.WriteLine("Assembly completed with warnings.");
                    }
                    else
                        Console.Error.WriteLine("Assembly completed successfully.");

                    if (assembler.OutputFormat == Format.Card && cmdLine.ContainsKey("--append-deck"))
                    {
                        string deckFile = cmdLine["--append-deck"];
                        Console.Error.WriteLine(string.Format("Appending deck file: '{0}'", deckFile));
                        StreamReader reader = new StreamReader(deckFile);
                        string deck = reader.ReadToEnd();
                        reader.Close();
                        StreamWriter writer = new StreamWriter(assembler.OutputFile == null ? Console.OpenStandardOutput() : new FileStream(assembler.OutputFile, FileMode.Append));
                        writer.Write(deck);
                        writer.Close();
                    }

                    if (cmdLine.ContainsKey("--pretty-print"))
                        assembler.PrettyPrinter = new TeXPrinter(assembler.InputFile);

                    assembler.MakeOther();
                }
            }
            catch (IOException e)
            {
                Console.Error.WriteLine("Input/output error: " + e.Message);
                Environment.Exit(-1);
            }
        }
    }
}
