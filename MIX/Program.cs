using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using MIXLib;
using MIXLib.Util;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

namespace MIX
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

        static void Main(string[] args)
        {
            Dictionary<string, string> aliases = new Dictionary<string, string>();
            aliases.Add("-d", "--deck");
            aliases.Add("-b", "--binary");
            aliases.Add("-?", "--help");
            aliases.Add("-h", "--help");

            Dictionary<string, string> cmdLine = CommandLineHelper.SplitCommandLine(Environment.CommandLine, aliases, true);
            if (cmdLine.Count == 0)
            {
                Console.WriteLine("This is MIX v0.1, (c) 2009 George Tryfonas\nAn mplementation of the machine described by Don Knuth.\n\nType '?' or 'help' at the prompt for instructions.\n");
                MIXController c = new MIXController();
                c.Interface();
            }
            else
            {
                Stream stream;

                MIXMachine machine = new MIXMachine();
                string inFile = GetInputFile(cmdLine);
                if (string.IsNullOrEmpty(inFile))
                    stream = Console.OpenStandardInput();
                else
                    stream = new FileStream(inFile, FileMode.Open);

                if (cmdLine.ContainsKey("--deck"))
                {
                    machine.RedirectDevice(MIXMachine.CARD_READER, stream);
                    machine.LoadDeck();
                }
                else if (cmdLine.ContainsKey("--binary"))
                {
                    IFormatter formatter = new BinaryFormatter();
                    MIXWord startLoc = (MIXWord)formatter.Deserialize(stream);
                    List<MemoryCell> data = (List<MemoryCell>)formatter.Deserialize(stream);

                    machine.LoadImage(data);
                    machine.PC = startLoc;
                    machine.Run();
                }
            }
        }
    }
}
