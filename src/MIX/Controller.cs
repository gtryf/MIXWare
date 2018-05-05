using System;
using System.IO;
using MIXLib;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Collections.Generic;
using System.Linq;

namespace MIX
{
    public class MIXController
    {
        MIXMachine machine;
        Dictionary<string, MIXWord> symbolTable;

        public MIXController()
        {
            machine = new MIXMachine();
            verbose = false;
            symbolTable = new Dictionary<string, MIXWord>();
        }

        public void Interface()
        {
            string input = "";

            while (true)
            {
                // Get next command
                Console.Write("> ");
                input = Console.ReadLine();
                var commands = input.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);

                if (commands.Length > 0)
                {
                    switch (commands[0].Trim().ToLower())
                    {
                        case "?":
                        case "help":
                            Help(commands.Skip(1).ToArray());
                            break;
                        case "reboot":
                            Console.WriteLine("Rebooting...");
                            machine = new MIXMachine();
                            symbolTable = new Dictionary<string, MIXWord>();
                            Console.WriteLine("Done.");
                            break;
                        case "quit":
                        case "exit":
                            Console.WriteLine("Bye.");
                            Environment.Exit(0);
                            break;
                        case "load":
                            int w = LoadImage(commands[1]);
                            Console.WriteLine(string.Format("Loaded {0} word(s). PC set to {1}", w, machine.PC));
                            break;
                        case "loaddeck":
                            LoadDeck(commands[1]);
                            break;
                        case "verbose":
                            verbose = !verbose;
                            break;
                        case "show":
                            Show(commands.Skip(1).ToArray());
                            break;
                        case "set":
                            Set(commands.Skip(1).ToArray());
                            break;
                        case "clear":
                            Clear(commands.Skip(1).ToArray());
                            break;
                        case "step":
                            Console.WriteLine(string.Format("PC: {1} - Executing: {0}", GetDisassembly(machine.Memory[machine.PC]), machine.PC));
                            machine.Step();
                            if (verbose)
                                ShowAllState();
                            Console.WriteLine("Done.");
                            break;
                        case "go":
                        case "run":
                            Console.WriteLine("Executing...");
                            machine.Run();
                            if (verbose)
                                ShowAllState();
                            Console.WriteLine("Done.");
                            break;
                        case "redir":
                        case "redirect":
                            Redirect(commands.Skip(1).ToArray());
                            break;
                        default:
                            Console.Error.WriteLine("Unknown command.");
                            break;
                    }
                }
            }
        }

        private void Help(string[] what)
        {
            if (what.Length == 0)
            {
                Console.WriteLine("Commands are:\n");

                Console.WriteLine("CLEAR (BP|BREAKPOINT) [n|ALL]\n\tClear breakpoint n, or all breakpoints.");
                Console.WriteLine("EXIT|QUIT\n\tExit the console.");
                Console.WriteLine("GO|RUN\n\tStart execution.");
                Console.WriteLine("LOAD {filename}\n\tLoad a memory dump from file.");
                Console.WriteLine("REBOOT\n\tRestart MIX.");
                Console.WriteLine("REDIR[ECT] {UnitId} {filename|console}\n\tRedirect I/O device UnitId to the console or to a disk file.");
                Console.WriteLine("SET {args}\n\tSet various parameters. Type ? SET for more info.");
                Console.WriteLine("SHOW {args}\n\tShow the current values of various parameters. Type ? SHOW for more info.");
                Console.WriteLine("STEP\n\tExecute only the current instruction.");
                Console.WriteLine("VERBOSE\n\tToggle verbosity on or off.");
            }
            else
            {
                switch (what[0])
                {
                    case "show":
                        Console.WriteLine("Valid options are:\n");
                        Console.WriteLine("TIME\n\t Show execution time in time units.");
                        Console.WriteLine("BREAKPOINT|BP [n]\n\tShow breakpoint n, if specified, otherwise show all breakpoints.");
                        Console.WriteLine("MEM|MEMORY [start end [WITH (DASM|DISASSEMBLY)]]\n\tShow contents of memory from 'start' to 'end'\n\twith optional disassembly.\n\n\tNote: 'start' or 'end' may be a symbol.");
                        Console.WriteLine("r(A|X|J|I1-I6)\n\tShow contents of specified register.");
                        Console.WriteLine("OF|OVERFLOW\n\tShow status of the overflow flag.");
                        Console.WriteLine("CI\n\tShow status of the comparison indicator.");
                        Console.WriteLine("PC\n\tShow contents of program counter.");
                        Console.WriteLine("STATE\n\tShow contents of all registers, flags and indicators.");
                        Console.WriteLine("VERBOSE\n\tShow status of the verbose flag.");
                        Console.WriteLine("DEVICE [n]\n\tShow information about device n, if specified,\n\totherwise show information for all devices.");
                        Console.WriteLine("SYMBOL {symb}\n\tShow the value of symbol 'symb'.");
                        Console.WriteLine("SYMBOLS\n\tShow all the contents of the symbol table.");
                        break;
                    case "set":
                        Console.WriteLine("Valid options are:\n");
                        Console.WriteLine("MEM|MEMORY {loc} {data}\n\tSet contents of memory at location 'loc' to 'data'.");
                        Console.WriteLine("r(A|X|J|I1-I6|PC) {data}\n\tSet contents of specified register to 'data'.");
                        Console.WriteLine("BP|BREAKPOINT {loc}\n\tSet a new breakpoint at location 'loc'.\n\n\tNote: 'loc' can be a symbol.");
                        break;
                    default:
                        Console.Error.WriteLine("Unknown command.");
                        break;
                }
            }
        }

        private void Redirect(string[] what)
        {
            int unitId = int.Parse(what[0]);
            if (what[1] == "console" || what[1] == "stdout")
            {
                machine.RedirectDevice(unitId, Console.OpenStandardOutput());
            }
            else if (what[1] == "stdout")
            {
                machine.RedirectDevice(unitId, Console.OpenStandardInput());
            }
            else
            {
                machine.RedirectDevice(unitId, null);
                FileStream fs;
                if (unitId == MIXMachine.CARD_READER)
                    fs = new FileStream(what[1], FileMode.Open);
                else
                    fs = new FileStream(what[1], FileMode.Append);
                machine.RedirectDevice(unitId, fs);
            }
        }

        private void LoadDeck(string deck)
        {
            FileStream deckStream = new FileStream(deck, FileMode.Open);
            machine.RedirectDevice(MIXMachine.CARD_READER, deckStream);
            machine.LoadDeck();
            deckStream.Close();
            machine.RedirectDevice(MIXMachine.CARD_READER, null);
        }

        private int LoadImage(string dump)
        {
            IFormatter formatter = new BinaryFormatter();
            Stream stream = new FileStream(dump, FileMode.Open, FileAccess.Read, FileShare.Read);
            MIXWord startLoc = (MIXWord)formatter.Deserialize(stream);
            List<MemoryCell> data = (List<MemoryCell>)formatter.Deserialize(stream);
            symbolTable = (Dictionary<string, MIXWord>)formatter.Deserialize(stream);
            stream.Close();

            machine.LoadImage(data);
            machine.PC = startLoc;

            return data.Count;
        }

        private void Clear(string[] what)
        {
            switch (what[0])
            {
                case "bp":
                case "breakpoint":
                    if (what.Length == 1 || what[1] == "all")
                        machine.ClearAllBreakpoints();
                    else
                        machine.ClearBreakpoint(int.Parse(what[1]));
                    break;

                default:
                    Console.Error.WriteLine("Unknown parameter.");
                    break;
            }
        }

        private void Show(string[] what)
        {
            switch (what[0])
            {
                case "time":
                    Console.WriteLine(string.Format("Execution time: {0}u", machine.ExecutionTime));
                    break;
                case "bp":
                case "breakpoint":
                    if (what.Length == 1)
                    {
                        for (int i = 0; i < machine.BreakpointCount; i++)
                            Console.WriteLine(string.Format("{0} @ {1}", i, machine.GetBreakpoint(i)));
                    }
                    else
                    {
                        int bp = int.Parse(what[1]);
                        Console.WriteLine(string.Format("{0} @ {1}", bp, machine.GetBreakpoint(bp)));
                    }
                    break;
                case "mem":
                case "memory":
                    int start = 0, end = 3999;
                    if (what.Length > 1)
                    {
                        if (symbolTable.ContainsKey(what[1].ToUpper()))
                            start = symbolTable[what[1].ToUpper()].Value;
                        else
                            start = int.Parse(what[1]);
                        if (what.Length > 2)
                        {
                            if (symbolTable.ContainsKey(what[2].ToUpper()))
                                end = symbolTable[what[2].ToUpper()].Value;
                            else
                                end = int.Parse(what[2]);
                        }
                    }

                    bool dasm = false;
                    if (what.Length > 4 && what[3] == "with" && (what[4] == "disassembly" || what[4] == "dasm"))
                        dasm = true;

                    ShowMemory(start, end, dasm);
                    break;
                case "state":
                    ShowAllState();
                    break;
                case "ra":
                    ShowRegister(7);
                    break;
                case "rx":
                    ShowRegister(8);
                    break;
                case "rj":
                    ShowRegister(9);
                    break;
                case "ri1":
                    ShowRegister(1);
                    break;
                case "ri2":
                    ShowRegister(2);
                    break;
                case "ri3":
                    ShowRegister(3);
                    break;
                case "ri4":
                    ShowRegister(4);
                    break;
                case "ri5":
                    ShowRegister(5);
                    break;
                case "ri6":
                    ShowRegister(6);
                    break;
                case "overflow":
                case "of":
                    ShowRegister(10);
                    break;
                case "ci":
                    ShowRegister(11);
                    break;
                case "pc":
                    ShowRegister(12);
                    break;
                case "verbose":
                    Console.WriteLine(string.Format("Verbose mode: {0}", verbose));
                    break;
                case "devices":
                    ShowAllDevices();
                    break;
                case "device":
                    if (what.Length > 1)
                        ShowDevice(int.Parse(what[1]));
                    else
                        ShowAllDevices();
                    break;
                case "symbols":
                    foreach (var s in symbolTable)
                    {
                        Console.WriteLine(string.Format("{0} = {1} = {2} = '{3}'", s.Key, s.Value, s.Value.Value, MIXWordToString(s.Value)));
                    }
                    break;
                case "symbol":
                    if (symbolTable.ContainsKey(what[1].ToUpper()))
                    {
                        MIXWord w = symbolTable[what[1].ToUpper()];
                        Console.WriteLine(string.Format("{0} = {1} = {2} = '{3}'", what[1].ToUpper(), w, w.Value, MIXWordToString(w)));
                    }
                    else
                    {
                        Console.Error.WriteLine(string.Format("Symbol '{0}' not found.", what[1].ToUpper()));
                    }
                    break;
                default:
                    Console.Error.WriteLine(string.Format("Unknown parameter: '{0}'", what));
                    break;
            }
        }

        private void ShowDevice(int unitId)
        {
            Console.WriteLine(string.Format("UNIT #{0}: {1}", unitId, machine.GetDeviceInfo(unitId)));
        }

        private void ShowAllDevices()
        {
            for (int i = 0; i < 21; i++)
                ShowDevice(i);
        }

        private void ShowMemory(int start, int end, bool dasm)
        {
            Console.WriteLine(string.Format("MEMORY CONTENTS ({0:0000} TO {1:0000})", start, end));
            Console.WriteLine();
            for (int i = start; i <= end; i++)
            {
                MIXWord w = machine.Memory[i];
                if (!dasm)
                    Console.WriteLine(string.Format("@{0:0000}: {1} = {2} = '{3}'", i, w, Convert.ToString(w.Value).PadLeft(10), MIXWordToString(w)));
                else
                {
                    Console.WriteLine(string.Format("@{0:0000}: {1} = {2} = '{3}'\t{4}", i, w, Convert.ToString(w.Value).PadLeft(10), MIXWordToString(w), GetDisassembly(w)));
                }
            }
        }

        private string GetDisassembly(MIXWord w)
        {
            string disassembly = "";
            InstructionInfo info = machine.Disassemble(w);
            if (info == null)
                disassembly = "???";
            else
            {
                disassembly = machine.Disassemble(w).Name + " " + w[0, 2];
                disassembly += w[3] != 0 ? "," + w[3] : "";
                if (w[4] != machine.Disassemble(w).DefaultField)
                    disassembly += "(" + (w[4] / 8) + ":" + (w[4] % 8) + ")";
            }

            return disassembly;
        }

        private void ShowAllState()
        {
            ShowRegister(7);
            ShowRegister(8);
            ShowRegister(9);

            ShowRegister(1);
            ShowRegister(2);
            ShowRegister(3);
            ShowRegister(4);
            ShowRegister(5);
            ShowRegister(6);

            ShowRegister(10);
            ShowRegister(11);
            ShowRegister(12);
        }

        private void ShowRegister(byte which)
        {
            switch (which)
            {
                case 1:
                case 2:
                case 3:
                case 4:
                case 5:
                case 6:
                    Console.WriteLine(string.Format("I{0}: {1} = {2} = '{3}'", which, machine.I[which - 1], machine.I[which - 1].Value, MIXWordToString(machine.I[which - 1])));
                    break;

                case 7: Console.WriteLine(string.Format(" A: {0} = {1} = '{2}'", machine.A, machine.A.Value, MIXWordToString(machine.A)));
                    break;
                case 8: Console.WriteLine(string.Format(" X: {0} = {1} = '{2}'", machine.X, machine.X.Value, MIXWordToString(machine.X)));
                    break;
                case 9: Console.WriteLine(string.Format(" J: {0} = {1} = '{2}'", machine.J, machine.J.Value, MIXWordToString(machine.J)));
                    break;
                case 10: Console.WriteLine(string.Format("Overflow: {0}", machine.Overflow));
                    break;
                case 11: Console.WriteLine(string.Format("CI: {0}", machine.CI));
                    break;
                case 12: Console.WriteLine(string.Format("PC: {0}", machine.PC));
                    break;

                default:
                    Console.Error.WriteLine("Unknown register.");
                    break;
            }
        }

        private void Set(string[] what)
        {
            switch (what[0])
            {
                case "mem":
                case "memory":
                    machine.Memory[int.Parse(what[1])] = new MIXWord(int.Parse(what[2]));
                    break;
                case "ra":
                    SetRegister(7, int.Parse(what[1]));
                    break;
                case "rx":
                    SetRegister(8, int.Parse(what[1]));
                    break;
                case "rj":
                    SetRegister(9, int.Parse(what[1]));
                    break;
                case "ri1":
                    SetRegister(1, int.Parse(what[1]));
                    break;
                case "ri2":
                    SetRegister(2, int.Parse(what[1]));
                    break;
                case "ri3":
                    SetRegister(3, int.Parse(what[1]));
                    break;
                case "ri4":
                    SetRegister(4, int.Parse(what[1]));
                    break;
                case "ri5":
                    SetRegister(5, int.Parse(what[1]));
                    break;
                case "ri6":
                    SetRegister(6, int.Parse(what[1]));
                    break;
                case "pc":
                    SetRegister(12, int.Parse(what[1]));
                    break;
                case "bp":
                case "breakpoint":
                    if (symbolTable.ContainsKey(what[1].ToUpper()))
                        machine.AddBreakpoint(symbolTable[what[1].ToUpper()]);
                    else
                        machine.AddBreakpoint(int.Parse(what[1]));
                    break;
                default:
                    Console.Error.WriteLine(string.Format("Unknown parameter: '{0}'", what));
                    break;
            }
        }

        private void SetRegister(int which, int value)
        {
            switch (which)
            {
                case 1:
                case 2:
                case 3:
                case 4:
                case 5:
                case 6:
                    machine.I[which - 1].Value = value;
                    break;

                case 7: machine.A.Value = value;
                    break;
                case 8: machine.X.Value = value;
                    break;
                case 9: machine.J.Value = value;
                    break;
                case 12: machine.PC = value;
                    break;

                default:
                    Console.Error.WriteLine("Unknown register.");
                    return;
            }
            Console.WriteLine("Done.");
        }

        private bool verbose;

        private string MIXWordToString(MIXWord w)
        {
            string result = "";

            for (byte i = 1; i < 6; i++)
            {
                var ch = from c in MIXMachine.CHAR_TABLE
                         where w[i] == c.Value
                         select c.Key;
                if (ch.Count() > 0)
                    result += ch.ElementAt(0).ToString();
                else
                    result += "█";
            }

            return result;
        }
    }
}