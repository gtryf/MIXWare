using System.Collections.Generic;
using System.Linq;
using System;
using System.IO;

namespace MIXLib
{
    public enum Comparison
    {
        LESS,
        GREATER,
        EQUAL
    }

    public class MIXMachine
    {
        #region Initialization

        public MIXMachine()
        {
            MakeInstructionTable();
            InitMachine();
            InitDevices();
        }

        private void InitMachine()
        {
            Memory = new MIXWord[4000];
            Breakpoints = new List<int>();

            for (var i = 0; i < 4000; i++)
                Memory[i] = new MIXWord();

            PC = 0;

            A = new MIXWord();
            X = new MIXWord();
            J = new MIXWord();

            I = new MIXWord[6];
            for (int i = 0; i < 6; i++)
                I[i] = new MIXWord();

            Overflow = false;
            CI = Comparison.EQUAL;

            Running = false;
            ExecutionTime = 0;
        }

        #endregion

        #region Memory

        public MIXWord[] Memory;

        public void LoadImage(List<MemoryCell> dump)
        {
            foreach (var c in dump)
                Memory[c.Location] = c.Contents;
        }

        public void LoadDeck()
        {
            var inInstr = instructionTable.Find(i => i.Name == "IN");
            inInstr.Execute(new MIXWord(), 0, CARD_READER);

            PC = 0;
            J.Value = 0;

            Run();
        }

        public InstructionInfo Disassemble(MIXWord w)
        {
            var first = INSTRUCTION_LIST.Find(l => l.OpCode == w[5]);
            if (first == INSTRUCTION_LIST.FindLast(l => l.OpCode == w[5]))
                return first;
            else
                return INSTRUCTION_LIST.Find(l => l.OpCode == w[5] && l.DefaultField == w[4]);
        }

        #endregion

        #region Devices

        // Block devices
        public const byte TAPE1 = 0;
        public const byte TAPE2 = 1;
        public const byte TAPE3 = 2;
        public const byte TAPE4 = 3;
        public const byte TAPE5 = 4;
        public const byte TAPE6 = 5;
        public const byte TAPE7 = 6;
        public const byte TAPE8 = 7;

        public const byte DISK1 = 8;
        public const byte DISK2 = 9;
        public const byte DISK3 = 10;
        public const byte DISK4 = 11;
        public const byte DISK5 = 12;
        public const byte DISK6 = 13;
        public const byte DISK7 = 14;
        public const byte DISK8 = 15;

        // Character devices
        public const byte CARD_READER = 16;
        public const byte CARD_PUNCH = 17;
        public const byte LINE_PRINTER = 18;
        public const byte TERMINAL = 19;
        public const byte PAPER_TAPE = 20;

        private MIXDevice[] devices;

        private void InitDevices()
        {
            devices = new MIXDevice[21];

            devices[CARD_READER] = new CardReader(this, null);
            devices[CARD_PUNCH] = new CardPunch(this, null);
            devices[LINE_PRINTER] = new LinePrinter(this, Console.OpenStandardOutput());
            devices[TERMINAL] = new Terminal(this, new MemoryStream());
            devices[PAPER_TAPE] = new PaperTape(this, null);
        }

        public void RedirectDevice(int unitId, Stream newStore)
        {
            devices[unitId].Redirect(newStore);
        }

        public string GetDeviceInfo(int unitId)
        {
            if (devices[unitId] == null)
                return "NOT INSTALLED";

            return devices[unitId].ToString();
        }

        #endregion

        #region Execution

        public bool Running { get; set; }
        private List<int> Breakpoints { get; set; }
        public int ExecutionTime { get; private set; }

        public void Run()
        {
            if (!Running)
                Running = true;

            ExecutionTime = 0;
            while (Running && PC < 4000)
                Step();
        }

        public void AddBreakpoint(int location) => Breakpoints.Add(location);

        public void ClearBreakpoint(int which) => Breakpoints.RemoveAt(which);

        public void ClearAllBreakpoints() => Breakpoints.Clear();

        public int GetBreakpoint(int i) => Breakpoints[i];

        public int BreakpointCount => Breakpoints.Count;

        public void Step()
        {
            MIXWord w = Memory[PC];

            InstructionInfo info = Disassemble(w);
            if (info != null)
            {
                InstructionInfo ii = info;
                var ilist = from i in instructionTable
                            where i.Name == ii.Name
                            select i;

                if (ilist.Count() == 0)
                    Console.Error.WriteLine(string.Format("Instruction not found: '{0}'", ii.Name));
                else
                {
                    var instr = ilist.ElementAt(0);
                    instr.Execute(new MIXWord(w[0, 2]), (byte)w[3], (byte)w[4]);
                    ExecutionTime += ii.Time;
                }
            }

            if (Breakpoints.Contains(PC))
                Running = false;
        }

        #endregion

        #region Registers

        public int PC { get; set; }

        public MIXWord A { get; private set; }
        public MIXWord X { get; private set; }
        public MIXWord J { get; private set; }

        public MIXWord[] I { get; private set; }

        public bool Overflow { get; private set; }
        public Comparison CI { get; private set; }

        #endregion

        #region Instructions

        private List<MIXInstruction> instructionTable;

        private void MakeInstructionTable()
        {
            instructionTable = new List<MIXInstruction>()
            {
                #region Load Operators

                new MIXInstruction("LDA",
                    (a, i, f) =>
                    {
                        int M = a + (i > 0 ? I[i - 1].Value : 0);
                        A.Value = Memory[M][(byte)(f / 8), (byte)(f % 8)];
                        PC++;
                    }),
                new MIXInstruction("LDX",
                    (a, i, f) =>
                    {
                        int M = a + (i > 0 ? I[i - 1].Value : 0);
                        X.Value = Memory[M][(byte)(f / 8), (byte)(f % 8)];
                        PC++;
                    }),
                new MIXInstruction("LD1",
                    (a, i, f) =>
                    {
                        int M = a + (i > 0 ? I[i - 1].Value : 0);
                        I[0].Value = Memory[M][(byte)(f / 8), (byte)(f % 8)];
                        PC++;
                    }),
                new MIXInstruction("LD2",
                    (a, i, f) =>
                    {
                        int M = a + (i > 0 ? I[i - 1].Value : 0);
                        I[1].Value = Memory[M][(byte)(f / 8), (byte)(f % 8)];
                        PC++;
                    }),
                new MIXInstruction("LD3",
                    (a, i, f) =>
                    {
                        int M = a + (i > 0 ? I[i - 1].Value : 0);
                        I[2].Value = Memory[M][(byte)(f / 8), (byte)(f % 8)];
                        PC++;
                    }),
                new MIXInstruction("LD4",
                    (a, i, f) =>
                    {
                        int M = a + (i > 0 ? I[i - 1].Value : 0);
                        I[3].Value = Memory[M][(byte)(f / 8), (byte)(f % 8)];
                        PC++;
                    }),
                new MIXInstruction("LD5",
                    (a, i, f) =>
                    {
                        int M = a + (i > 0 ? I[i - 1].Value : 0);
                        I[4].Value = Memory[M][(byte)(f / 8), (byte)(f % 8)];
                        PC++;
                    }),
                new MIXInstruction("LD6",
                    (a, i, f) =>
                    {
                        int M = a + (i > 0 ? I[i - 1].Value : 0);
                        I[5].Value = Memory[M][(byte)(f / 8), (byte)(f % 8)];
                        PC++;
                    }),
                new MIXInstruction("LDAN",
                    (a, i, f) =>
                    {
                        int M = a + (i > 0 ? I[i - 1].Value : 0);
                        A.Value = -Memory[M][(byte)(f / 8), (byte)(f % 8)];
                        PC++;
                    }),
                new MIXInstruction("LDXN",
                    (a, i, f) =>
                    {
                        int M = a + (i > 0 ? I[i - 1].Value : 0);
                        X.Value = -Memory[M][(byte)(f / 8), (byte)(f % 8)];
                        PC++;
                    }),
                new MIXInstruction("LD1N",
                    (a, i, f) =>
                    {
                        int M = a + (i > 0 ? I[i - 1].Value : 0);
                        I[0].Value = -Memory[M][(byte)(f / 8), (byte)(f % 8)];
                        PC++;
                    }),
                new MIXInstruction("LD2N",
                    (a, i, f) =>
                    {
                        int M = a + (i > 0 ? I[i - 1].Value : 0);
                        I[1].Value = -Memory[M][(byte)(f / 8), (byte)(f % 8)];
                        PC++;
                    }),
                new MIXInstruction("LD3N",
                    (a, i, f) =>
                    {
                        int M = a + (i > 0 ? I[i - 1].Value : 0);
                        I[2].Value = -Memory[M][(byte)(f / 8), (byte)(f % 8)];
                        PC++;
                    }),
                new MIXInstruction("LD4N",
                    (a, i, f) =>
                    {
                        int M = a + (i > 0 ? I[i - 1].Value : 0);
                        I[3].Value = -Memory[M][(byte)(f / 8), (byte)(f % 8)];
                        PC++;
                    }),
                new MIXInstruction("LD5N",
                    (a, i, f) =>
                    {
                        int M = a + (i > 0 ? I[i - 1].Value : 0);
                        I[4].Value = -Memory[M][(byte)(f / 8), (byte)(f % 8)];
                        PC++;
                    }),
                new MIXInstruction("LD6N",
                    (a, i, f) =>
                    {
                        int M = a + (i > 0 ? I[i - 1].Value : 0);
                        I[5].Value = -Memory[M][(byte)(f / 8), (byte)(f % 8)];
                        PC++;
                    }),

                #endregion

                #region Store Operators

                new MIXInstruction("STA",
                    (a, i, f) =>
                    {
                        int M = a + (i > 0 ? I[i - 1].Value : 0);
                        if (f > 0)
                        {
                            byte l = (byte)(f / 8); byte r = (byte)(f % 8);
                            bool useSign = false;
                            if (l == 0)
                            {
                                l++;
                                useSign = true;
                            }
                            byte width = (byte)(r - l);
                            int V = A[(byte)(5 - width), 5];

                            Memory[M][l, r] = V;
                            if (useSign)
                                Memory[M].Sign = A.Sign;
                        }
                        else
                            Memory[M].Sign = A.Sign;

                        PC++;
                    }),
                new MIXInstruction("STX",
                    (a, i, f) =>
                    {
                        int M = a + (i > 0 ? I[i - 1].Value : 0);
                        if (f > 0)
                        {
                            byte l = (byte)(f / 8); byte r = (byte)(f % 8);
                            bool useSign = false;
                            if (l == 0)
                            {
                                l++;
                                useSign = true;
                            }
                            byte width = (byte)(r - l);
                            int V = X[(byte)(5 - width), 5];

                            Memory[M][l, r] = V;
                            if (useSign)
                                Memory[M].Sign = X.Sign;
                        }
                        else
                            Memory[M].Sign = X.Sign;
                        PC++;
                    }),
                new MIXInstruction("ST1",
                    (a, i, f) =>
                    {
                        int M = a + (i > 0 ? I[i - 1].Value : 0);
                        if (f > 0)
                        {
                            byte l = (byte)(f / 8); byte r = (byte)(f % 8);
                            bool useSign = false;
                            if (l == 0)
                            {
                                l++;
                                useSign = true;
                            }
                            byte width = (byte)(r - l);
                            int V = I[0][(byte)(5 - width), 5];

                            Memory[M][l, r] = V;
                            if (useSign)
                                Memory[M].Sign = I[0].Sign;
                        }
                        else
                            Memory[M].Sign = I[0].Sign;
                        PC++;
                    }),
                new MIXInstruction("ST2",
                    (a, i, f) =>
                    {
                        int M = a + (i > 0 ? I[i - 1].Value : 0);
                        if (f > 0)
                        {
                            byte l = (byte)(f / 8); byte r = (byte)(f % 8);
                            bool useSign = false;
                            if (l == 0)
                            {
                                l++;
                                useSign = true;
                            }
                            byte width = (byte)(r - l);
                            int V = I[1][(byte)(5 - width), 5];

                            Memory[M][l, r] = V;
                            if (useSign)
                                Memory[M].Sign = I[1].Sign;
                        }
                        else
                            Memory[M].Sign = I[1].Sign;
                        PC++;
                    }),
                new MIXInstruction("ST3",
                    (a, i, f) =>
                    {
                        int M = a + (i > 0 ? I[i - 1].Value : 0);
                        if (f > 0)
                        {
                            byte l = (byte)(f / 8); byte r = (byte)(f % 8);
                            bool useSign = false;
                            if (l == 0)
                            {
                                l++;
                                useSign = true;
                            }
                            byte width = (byte)(r - l);
                            int V = I[2][(byte)(5 - width), 5];

                            Memory[M][l, r] = V;
                            if (useSign)
                                Memory[M].Sign = I[2].Sign;
                        }
                        else
                            Memory[M].Sign = I[2].Sign;
                        PC++;
                    }),
                new MIXInstruction("ST4",
                    (a, i, f) =>
                    {
                        int M = a + (i > 0 ? I[i - 1].Value : 0);
                        if (f > 0)
                        {
                            byte l = (byte)(f / 8); byte r = (byte)(f % 8);
                            bool useSign = false;
                            if (l == 0)
                            {
                                l++;
                                useSign = true;
                            }
                            byte width = (byte)(r - l);
                            int V = I[3][(byte)(5 - width), 5];

                            Memory[M][l, r] = V;
                            if (useSign)
                                Memory[M].Sign = I[3].Sign;
                        }
                        else
                            Memory[M].Sign = I[3].Sign;
                        PC++;
                    }),
                new MIXInstruction("ST5",
                    (a, i, f) =>
                    {
                        int M = a + (i > 0 ? I[i - 1].Value : 0);
                        if (f > 0)
                        {
                            byte l = (byte)(f / 8); byte r = (byte)(f % 8);
                            bool useSign = false;
                            if (l == 0)
                            {
                                l++;
                                useSign = true;
                            }
                            byte width = (byte)(r - l);
                            int V = I[4][(byte)(5 - width), 5];

                            Memory[M][l, r] = V;
                            if (useSign)
                                Memory[M].Sign = I[4].Sign;
                        }
                        else
                            Memory[M].Sign = I[4].Sign;
                        PC++;
                    }),
                new MIXInstruction("ST6",
                    (a, i, f) =>
                    {
                        int M = a + (i > 0 ? I[i - 1].Value : 0);
                        if (f > 0)
                        {
                            byte l = (byte)(f / 8); byte r = (byte)(f % 8);
                            bool useSign = false;
                            if (l == 0)
                            {
                                l++;
                                useSign = true;
                            }
                            byte width = (byte)(r - l);
                            int V = I[5][(byte)(5 - width), 5];

                            Memory[M][l, r] = V;
                            if (useSign)
                                Memory[M].Sign = I[5].Sign;
                        }
                        else
                            Memory[M].Sign = I[5].Sign;
                        PC++;
                    }),
                new MIXInstruction("STJ",
                    (a, i, f) =>
                    {
                        int M = a + (i > 0 ? I[i - 1].Value : 0);
                        byte l = (byte)(f / 8); byte r = (byte)(f % 8);
                        byte width = (byte)(r - l);
                        int V = Math.Abs(J[(byte)(5 - width), 5]);

                        Memory[M][l, r] = V;
                        PC++;
                    }),
                new MIXInstruction("STZ",
                    (a, i, f) =>
                    {
                        int M = a + (i > 0 ? I[i - 1].Value : 0);
                        byte l = (byte)(f / 8); byte r = (byte)(f % 8);

                        Memory[M][l, r] = 0;
                        PC++;
                    }),

                #endregion

                #region Arithmetic

                new MIXInstruction("ADD",
                    (a, i, f) =>
                    {
                        int M = a + (i > 0 ? I[i - 1].Value : 0);
                        byte l = (byte)(f / 8); byte r = (byte)(f % 8);
                        int V = Memory[M][l, r];

                        if (Math.Abs(A.Value + V) > MIXWord.MaxValue)
                        {
                            Overflow = true;
                            int result = Convert.ToInt32(Convert.ToString(Math.Abs(A.Value + V), 2).Substring(1), 2);
                            if (A.Value + V < 0)
                                A.Sign = Sign.Negative;
                            else
                                A.Sign = Sign.Positive;
                            A.UnsignedValue = result;
                        }
                        else
                            A.Value += V;

                        PC++;
                    }),
                new MIXInstruction("SUB",
                    (a, i, f) =>
                    {
                        int M = a + (i > 0 ? I[i - 1].Value : 0);
                        byte l = (byte)(f / 8); byte r = (byte)(f % 8);
                        int V = Memory[M][l, r];

                        if (Math.Abs(A.Value - V) > MIXWord.MaxValue)
                        {
                            Overflow = true;
                            int result = Convert.ToInt32(Convert.ToString(Math.Abs(A.Value - V), 2).Substring(1), 2);
                            if (A.Value - V < 0)
                                A.Sign = Sign.Negative;
                            else
                                A.Sign = Sign.Positive;
                            A.UnsignedValue = result;
                        }
                        else
                            A.Value -= V;

                        PC++;
                    }),
                new MIXInstruction("MUL",
                    (a, i, f) =>
                    {
                        int M = a + (i > 0 ? I[i - 1].Value : 0);
                        byte l = (byte)(f / 8); byte r = (byte)(f % 8);
                        int V = Memory[M][l, r];
                        long result = (long)V * (long)A.Value;

                        Sign resSign = result < 0 ? Sign.Negative : Sign.Positive;
                        string resultStr = Convert.ToString(Math.Abs(result), 2).PadLeft(60, '0');

                        A.Value = Convert.ToInt32(resultStr.Substring(0, 30), 2); A.Sign = resSign;
                        X.Value = Convert.ToInt32(resultStr.Substring(30), 2); X.Sign = resSign;

                        PC++;
                    }),
                new MIXInstruction("DIV",
                    (a, i, f) =>
                    {
                        int M = a + (i > 0 ? I[i - 1].Value : 0);
                        byte l = (byte)(f / 8); byte r = (byte)(f % 8);
                        int V = Memory[M][l, r];

                        if (V == 0 || (Math.Abs(A.Value) >= Math.Abs(V)))
                            Overflow = true;
                        else
                        {
                            int sign = A[0];
                            string numerString = Convert.ToString(A.UnsignedValue, 2).PadLeft(30, '0') + Convert.ToString(X.UnsignedValue, 2).PadLeft(30, '0');
                            long numer = Convert.ToInt64(numerString, 2);
                            A.Value = (int)(numer / V);
                            X.Value = (int)(numer % V);
                            X[0] = sign;
                        }

                        PC++;
                    }),

                #endregion

                #region Address transfer

                new MIXInstruction("ENTA",
                    (a, i, f) =>
                    {
                        int M = a + (i > 0 ? I[i - 1].Value : 0);
                        A.Value = M;
                        if (a.Value == 0)
                            A.Sign = a.Sign;

                        PC++;
                    }),
                new MIXInstruction("ENTX",
                    (a, i, f) =>
                    {
                        int M = a + (i > 0 ? I[i - 1].Value : 0);
                        X.Value = M;
                        if (a.Value == 0)
                            X.Sign = a.Sign;

                        PC++;
                    }),
                new MIXInstruction("ENT1",
                    (a, i, f) =>
                    {
                        int M = a + (i > 0 ? I[i - 1].Value : 0);
                        I[0].Value = M;
                        if (a.Value == 0)
                            I[0].Sign = a.Sign;

                        PC++;
                    }),
                new MIXInstruction("ENT2",
                    (a, i, f) =>
                    {
                        int M = a + (i > 0 ? I[i - 1].Value : 0);
                        I[1].Value = M;
                        if (a.Value == 0)
                            I[0].Sign = a.Sign;

                        PC++;
                    }),
                new MIXInstruction("ENT3",
                    (a, i, f) =>
                    {
                        int M = a + (i > 0 ? I[i - 1].Value : 0);
                        I[2].Value = M;
                        if (a.Value == 0)
                            I[0].Sign = a.Sign;

                        PC++;
                    }),
                new MIXInstruction("ENT4",
                    (a, i, f) =>
                    {
                        int M = a + (i > 0 ? I[i - 1].Value : 0);
                        I[3].Value = M;
                        if (a.Value == 0)
                            I[0].Sign = a.Sign;

                        PC++;
                    }),
                new MIXInstruction("ENT5",
                    (a, i, f) =>
                    {
                        int M = a + (i > 0 ? I[i - 1].Value : 0);
                        I[4].Value = M;
                        if (a.Value == 0)
                            I[0].Sign = a.Sign;

                        PC++;
                    }),
                new MIXInstruction("ENT6",
                    (a, i, f) =>
                    {
                        int M = a + (i > 0 ? I[i - 1].Value : 0);
                        I[5].Value = M;
                        if (a.Value == 0)
                            I[0].Sign = a.Sign;

                        PC++;
                    }),
                new MIXInstruction("ENNA",
                    (a, i, f) =>
                    {
                        int M = a + (i > 0 ? I[i - 1].Value : 0);
                        A.Value = -M;

                        PC++;
                    }),
                new MIXInstruction("ENNX",
                    (a, i, f) =>
                    {
                        int M = a + (i > 0 ? I[i - 1].Value : 0);
                        X.Value = -M;

                        PC++;
                    }),
                new MIXInstruction("ENN1",
                    (a, i, f) =>
                    {
                        int M = a + (i > 0 ? I[i - 1].Value : 0);
                        I[0].Value = -M;

                        PC++;
                    }),
                new MIXInstruction("ENN2",
                    (a, i, f) =>
                    {
                        int M = a + (i > 0 ? I[i - 1].Value : 0);
                        I[1].Value = -M;

                        PC++;
                    }),
                new MIXInstruction("ENN3",
                    (a, i, f) =>
                    {
                        int M = a + (i > 0 ? I[i - 1].Value : 0);
                        I[2].Value = -M;

                        PC++;
                    }),
                new MIXInstruction("ENN4",
                    (a, i, f) =>
                    {
                        int M = a + (i > 0 ? I[i - 1].Value : 0);
                        I[3].Value = -M;

                        PC++;
                    }),
                new MIXInstruction("ENN5",
                    (a, i, f) =>
                    {
                        int M = a + (i > 0 ? I[i - 1].Value : 0);
                        I[4].Value = -M;

                        PC++;
                    }),
                new MIXInstruction("ENN6",
                    (a, i, f) =>
                    {
                        int M = a + (i > 0 ? I[i - 1].Value : 0);
                        I[5].Value = -M;

                        PC++;
                    }),
                new MIXInstruction("INCA",
                    (a, i, f) =>
                    {
                        int M = a + (i > 0 ? I[i - 1].Value : 0);
                        if (Math.Abs(A.Value + M) > MIXWord.MaxValue)
                        {
                            Overflow = true;
                            int result = Convert.ToInt32(Convert.ToString(Math.Abs(A.Value + M), 2).Substring(1), 2);
                            if (A.Value + M < 0)
                                A.Sign = Sign.Negative;
                            else
                                A.Sign = Sign.Positive;
                            A.UnsignedValue = result;
                        }
                        else
                            A.Value += M;

                        PC++;
                    }),
                new MIXInstruction("INCX",
                    (a, i, f) =>
                    {
                        int M = a + (i > 0 ? I[i - 1].Value : 0);
                        if (Math.Abs(X.Value + M) > MIXWord.MaxValue)
                        {
                            Overflow = true;
                            int result = Convert.ToInt32(Convert.ToString(Math.Abs(X.Value + M), 2).Substring(1), 2);
                            if (X.Value + M < 0)
                                X.Sign = Sign.Negative;
                            else
                                X.Sign = Sign.Positive;
                            X.UnsignedValue = result;
                        }
                        else
                            X.Value += M;

                        PC++;
                    }),
                new MIXInstruction("INC1",
                    (a, i, f) =>
                    {
                        int M = a + (i > 0 ? I[i - 1].Value : 0);
                        I[0].Value += M;

                        PC++;
                    }),
                new MIXInstruction("INC2",
                    (a, i, f) =>
                    {
                        int M = a + (i > 0 ? I[i - 1].Value : 0);
                        I[1].Value += M;

                        PC++;
                    }),
                new MIXInstruction("INC3",
                    (a, i, f) =>
                    {
                        int M = a + (i > 0 ? I[i - 1].Value : 0);
                        I[2].Value += M;

                        PC++;
                    }),
                new MIXInstruction("INC4",
                    (a, i, f) =>
                    {
                        int M = a + (i > 0 ? I[i - 1].Value : 0);
                        I[3].Value += M;

                        PC++;
                    }),
                new MIXInstruction("INC5",
                    (a, i, f) =>
                    {
                        int M = a + (i > 0 ? I[i - 1].Value : 0);
                        I[4].Value += M;

                        PC++;
                    }),
                new MIXInstruction("INC6",
                    (a, i, f) =>
                    {
                        int M = a + (i > 0 ? I[i - 1].Value : 0);
                        I[5].Value += M;

                        PC++;
                    }),
                new MIXInstruction("DECA",
                    (a, i, f) =>
                    {
                        int M = a + (i > 0 ? I[i - 1].Value : 0);
                        if (Math.Abs(A.Value - M) > MIXWord.MaxValue)
                        {
                            Overflow = true;
                            int result = Convert.ToInt32(Convert.ToString(Math.Abs(A.Value - M), 2).Substring(1), 2);
                            if ((A.Value - M) < 0)
                                A.Sign = Sign.Negative;
                            else
                                A.Sign = Sign.Positive;
                            A.UnsignedValue = result;
                        }
                        else
                            A.Value -= M;

                        PC++;
                    }),
                new MIXInstruction("DECX",
                    (a, i, f) =>
                    {
                        int M = a + (i > 0 ? I[i - 1].Value : 0);
                        if (Math.Abs(X.Value - M) > MIXWord.MaxValue)
                        {
                            Overflow = true;
                            int result = Convert.ToInt32(Convert.ToString(Math.Abs(X.Value - M), 2).Substring(1), 2);
                            if ((X.Value - M) < 0)
                                X.Sign = Sign.Negative;
                            else
                                X.Sign = Sign.Positive;
                            X.UnsignedValue = result;
                        }
                        else
                            X.Value -= M;

                        PC++;
                    }),
                new MIXInstruction("DEC1",
                    (a, i, f) =>
                    {
                        int M = a + (i > 0 ? I[i - 1].Value : 0);
                        I[0].Value -= M;

                        PC++;
                    }),
                new MIXInstruction("DEC2",
                    (a, i, f) =>
                    {
                        int M = a + (i > 0 ? I[i - 1].Value : 0);
                        I[1].Value -= M;

                        PC++;
                    }),
                new MIXInstruction("DEC3",
                    (a, i, f) =>
                    {
                        int M = a + (i > 0 ? I[i - 1].Value : 0);
                        I[2].Value -= M;

                        PC++;
                    }),
                new MIXInstruction("DEC4",
                    (a, i, f) =>
                    {
                        int M = a + (i > 0 ? I[i - 1].Value : 0);
                        I[3].Value -= M;

                        PC++;
                    }),
                new MIXInstruction("DEC5",
                    (a, i, f) =>
                    {
                        int M = a + (i > 0 ? I[i - 1].Value : 0);
                        I[4].Value -= M;

                        PC++;
                    }),
                new MIXInstruction("DEC6",
                    (a, i, f) =>
                    {
                        int M = a + (i > 0 ? I[i - 1].Value : 0);
                        I[5].Value -= M;

                        PC++;
                    }),

                #endregion

                #region Comparisons

                new MIXInstruction("CMPA",
                    (a, i, f) =>
                    {
                        int M = a + (i > 0 ? I[i - 1].Value : 0);
                        int aval = A[(byte)(f / 8), (byte)(f % 8)];
                        int mval = Memory[M][(byte)(f / 8), (byte)(f % 8)];

                        if (aval - mval > 0)
                            CI = Comparison.GREATER;
                        else if (aval - mval < 0)
                            CI = Comparison.LESS;
                        else
                            CI = Comparison.EQUAL;

                        PC++;
                    }),

                new MIXInstruction("CMPX",
                    (a, i, f) =>
                    {
                        int M = a + (i > 0 ? I[i - 1].Value : 0);
                        int aval = X[(byte)(f / 8), (byte)(f % 8)];
                        int mval = Memory[M][(byte)(f / 8), (byte)(f % 8)];

                        if (aval - mval > 0)
                            CI = Comparison.GREATER;
                        else if (aval - mval < 0)
                            CI = Comparison.LESS;
                        else
                            CI = Comparison.EQUAL;

                        PC++;
                    }),
                new MIXInstruction("CMP1",
                    (a, i, f) =>
                    {
                        int M = a + (i > 0 ? I[i - 1].Value : 0);
                        MIXWord j = new MIXWord(I[0][4, 5]);
                        int aval = j[(byte)(f / 8), (byte)(f % 8)];
                        int mval = Memory[M][(byte)(f / 8), (byte)(f % 8)];

                        if (aval - mval > 0)
                            CI = Comparison.GREATER;
                        else if (aval - mval < 0)
                            CI = Comparison.LESS;
                        else
                            CI = Comparison.EQUAL;

                        PC++;
                    }),
                new MIXInstruction("CMP2",
                    (a, i, f) =>
                    {
                        int M = a + (i > 0 ? I[i - 1].Value : 0);
                        MIXWord j = new MIXWord(I[1][4, 5]);
                        int aval = j[(byte)(f / 8), (byte)(f % 8)];
                        int mval = Memory[M][(byte)(f / 8), (byte)(f % 8)];

                        if (aval - mval > 0)
                            CI = Comparison.GREATER;
                        else if (aval - mval < 0)
                            CI = Comparison.LESS;
                        else
                            CI = Comparison.EQUAL;

                        PC++;
                    }),
                new MIXInstruction("CMP3",
                    (a, i, f) =>
                    {
                        int M = a + (i > 0 ? I[i - 1].Value : 0);
                        MIXWord j = new MIXWord(I[2][4, 5]);
                        int aval = j[(byte)(f / 8), (byte)(f % 8)];
                        int mval = Memory[M][(byte)(f / 8), (byte)(f % 8)];

                        if (aval - mval > 0)
                            CI = Comparison.GREATER;
                        else if (aval - mval < 0)
                            CI = Comparison.LESS;
                        else
                            CI = Comparison.EQUAL;

                        PC++;
                    }),
                new MIXInstruction("CMP4",
                    (a, i, f) =>
                    {
                        int M = a + (i > 0 ? I[i - 1].Value : 0);
                        MIXWord j = new MIXWord(I[3][4, 5]);
                        int aval = j[(byte)(f / 8), (byte)(f % 8)];
                        int mval = Memory[M][(byte)(f / 8), (byte)(f % 8)];

                        if (aval - mval > 0)
                            CI = Comparison.GREATER;
                        else if (aval - mval < 0)
                            CI = Comparison.LESS;
                        else
                            CI = Comparison.EQUAL;

                        PC++;
                    }),
                new MIXInstruction("CMP5",
                    (a, i, f) =>
                    {
                        int M = a + (i > 0 ? I[i - 1].Value : 0);
                        MIXWord j = new MIXWord(I[4][4, 5]);
                        int aval = j[(byte)(f / 8), (byte)(f % 8)];
                        int mval = Memory[M][(byte)(f / 8), (byte)(f % 8)];

                        if (aval - mval > 0)
                            CI = Comparison.GREATER;
                        else if (aval - mval < 0)
                            CI = Comparison.LESS;
                        else
                            CI = Comparison.EQUAL;

                        PC++;
                    }),
                new MIXInstruction("CMP6",
                    (a, i, f) =>
                    {
                        int M = a + (i > 0 ? I[i - 1].Value : 0);
                        MIXWord j = new MIXWord(I[5][4, 5]);
                        int aval = j[(byte)(f / 8), (byte)(f % 8)];
                        int mval = Memory[M][(byte)(f / 8), (byte)(f % 8)];

                        if (aval - mval > 0)
                            CI = Comparison.GREATER;
                        else if (aval - mval < 0)
                            CI = Comparison.LESS;
                        else
                            CI = Comparison.EQUAL;

                        PC++;
                    }),

                #endregion

                #region Jumps

                new MIXInstruction("JMP",
                    (a, i, f) =>
                    {
                        int M = a + (i > 0 ? I[i - 1].Value : 0);
                        J.Value = PC + 1;

                        PC = M;
                    }),
                new MIXInstruction("JSJ",
                    (a, i, f) =>
                    {
                        PC = a + (i > 0 ? I[i - 1].Value : 0);
                    }),
                new MIXInstruction("JOV",
                    (a, i, f) =>
                    {
                        int M = a + (i > 0 ? I[i - 1].Value : 0);
                        if (Overflow)
                        {
                            J.Value = PC + 1;
                            PC = M;
                        }
                        else
                            PC++;
                    }),
                new MIXInstruction("JNOV",
                    (a, i, f) =>
                    {
                        int M = a + (i > 0 ? I[i - 1].Value : 0);
                        if (!Overflow)
                        {
                            J.Value = PC + 1;
                            PC = M;
                        }
                        else
                        {
                            Overflow = false;
                            PC++;
                        }
                    }),
                new MIXInstruction("JL",
                    (a, i, f) =>
                    {
                        int M = a + (i > 0 ? I[i - 1].Value : 0);
                        if (CI == Comparison.LESS)
                        {
                            J.Value = PC + 1;
                            PC = M;
                        }
                        else
                            PC++;
                    }),
                new MIXInstruction("JG",
                    (a, i, f) =>
                    {
                        int M = a + (i > 0 ? I[i - 1].Value : 0);
                        if (CI == Comparison.GREATER)
                        {
                            J.Value = PC + 1;
                            PC = M;
                        }
                        else
                            PC++;
                    }),
                new MIXInstruction("JE",
                    (a, i, f) =>
                    {
                        int M = a + (i > 0 ? I[i - 1].Value : 0);
                        if (CI == Comparison.EQUAL)
                        {
                            J.Value = PC + 1;
                            PC = M;
                        }
                        else
                            PC++;
                    }),
                new MIXInstruction("JGE",
                    (a, i, f) =>
                    {
                        int M = a + (i > 0 ? I[i - 1].Value : 0);
                        if (CI != Comparison.LESS)
                        {
                            J.Value = PC + 1;
                            PC = M;
                        }
                        else
                            PC++;
                    }),
                new MIXInstruction("JNE",
                    (a, i, f) =>
                    {
                        int M = a + (i > 0 ? I[i - 1].Value : 0);
                        if (CI != Comparison.EQUAL)
                        {
                            J.Value = PC + 1;
                            PC = M;
                        }
                        else
                            PC++;
                    }),
                new MIXInstruction("JLE",
                    (a, i, f) =>
                    {
                        int M = a + (i > 0 ? I[i - 1].Value : 0);
                        if (CI != Comparison.GREATER)
                        {
                            J.Value = PC + 1;
                            PC = M;
                        }
                        else
                            PC++;
                    }),
                new MIXInstruction("JAN",
                    (a, i, f) =>
                    {
                        int M = a + (i > 0 ? I[i - 1].Value : 0);
                        if (A.Value < 0)
                        {
                            J.Value = PC + 1;
                            PC = M;
                        }
                        else
                            PC++;
                    }),
                new MIXInstruction("JAZ",
                    (a, i, f) =>
                    {
                        int M = a + (i > 0 ? I[i - 1].Value : 0);
                        if (A.Value == 0)
                        {
                            J.Value = PC + 1;
                            PC = M;
                        }
                        else
                            PC++;
                    }),
                new MIXInstruction("JAP",
                    (a, i, f) =>
                    {
                        int M = a + (i > 0 ? I[i - 1].Value : 0);
                        if (A.Value > 0)
                        {
                            J.Value = PC + 1;
                            PC = M;
                        }
                        else
                            PC++;
                    }),
                new MIXInstruction("JANN",
                    (a, i, f) =>
                    {
                        int M = a + (i > 0 ? I[i - 1].Value : 0);
                        if (A.Value >= 0)
                        {
                            J.Value = PC + 1;
                            PC = M;
                        }
                        else
                            PC++;
                    }),
                new MIXInstruction("JANP",
                    (a, i, f) =>
                    {
                        int M = a + (i > 0 ? I[i - 1].Value : 0);
                        if (A.Value <= 0)
                        {
                            J.Value = PC + 1;
                            PC = M;
                        }
                        else
                            PC++;
                    }),
                new MIXInstruction("JANZ",
                    (a, i, f) =>
                    {
                        int M = a + (i > 0 ? I[i - 1].Value : 0);
                        if (A.Value != 0)
                        {
                            J.Value = PC + 1;
                            PC = M;
                        }
                        else
                            PC++;
                    }),
                new MIXInstruction("JXN",
                    (a, i, f) =>
                    {
                        int M = a + (i > 0 ? I[i - 1].Value : 0);
                        if (X.Value < 0)
                        {
                            J.Value = PC + 1;
                            PC = M;
                        }
                        else
                            PC++;
                    }),
                new MIXInstruction("JXZ",
                    (a, i, f) =>
                    {
                        int M = a + (i > 0 ? I[i - 1].Value : 0);
                        if (X.Value == 0)
                        {
                            J.Value = PC + 1;
                            PC = M;
                        }
                        else
                            PC++;
                    }),
                new MIXInstruction("JXP",
                    (a, i, f) =>
                    {
                        int M = a + (i > 0 ? I[i - 1].Value : 0);
                        if (X.Value > 0)
                        {
                            J.Value = PC + 1;
                            PC = M;
                        }
                        else
                            PC++;
                    }),
                new MIXInstruction("JXNN",
                    (a, i, f) =>
                    {
                        int M = a + (i > 0 ? I[i - 1].Value : 0);
                        if (X.Value >= 0)
                        {
                            J.Value = PC + 1;
                            PC = M;
                        }
                        else
                            PC++;
                    }),
                new MIXInstruction("JXNP",
                    (a, i, f) =>
                    {
                        int M = a + (i > 0 ? I[i - 1].Value : 0);
                        if (X.Value <= 0)
                        {
                            J.Value = PC + 1;
                            PC = M;
                        }
                        else
                            PC++;
                    }),
                new MIXInstruction("JXNZ",
                    (a, i, f) =>
                    {
                        int M = a + (i > 0 ? I[i - 1].Value : 0);
                        if (X.Value != 0)
                        {
                            J.Value = PC + 1;
                            PC = M;
                        }
                        else
                            PC++;
                    }),

                new MIXInstruction("J1N",
                    (a, i, f) =>
                    {
                        int M = a + (i > 0 ? I[i - 1].Value : 0);
                        if (I[0].Value < 0)
                        {
                            J.Value = PC + 1;
                            PC = M;
                        }
                        else
                            PC++;
                    }),
                new MIXInstruction("J1Z",
                    (a, i, f) =>
                    {
                        int M = a + (i > 0 ? I[i - 1].Value : 0);
                        if (I[0].Value == 0)
                        {
                            J.Value = PC + 1;
                            PC = M;
                        }
                        else
                            PC++;
                    }),
                new MIXInstruction("J1P",
                    (a, i, f) =>
                    {
                        int M = a + (i > 0 ? I[i - 1].Value : 0);
                        if (I[0].Value > 0)
                        {
                            J.Value = PC + 1;
                            PC = M;
                        }
                        else
                            PC++;
                    }),
                new MIXInstruction("J1NN",
                    (a, i, f) =>
                    {
                        int M = a + (i > 0 ? I[i - 1].Value : 0);
                        if (I[0].Value >= 0)
                        {
                            J.Value = PC + 1;
                            PC = M;
                        }
                        else
                            PC++;
                    }),
                new MIXInstruction("J1NP",
                    (a, i, f) =>
                    {
                        int M = a + (i > 0 ? I[i - 1].Value : 0);
                        if (I[0].Value <= 0)
                        {
                            J.Value = PC + 1;
                            PC = M;
                        }
                        else
                            PC++;
                    }),
                new MIXInstruction("J1NZ",
                    (a, i, f) =>
                    {
                        int M = a + (i > 0 ? I[i - 1].Value : 0);
                        if (I[0].Value != 0)
                        {
                            J.Value = PC + 1;
                            PC = M;
                        }
                        else
                            PC++;
                    }),
                new MIXInstruction("J2N",
                    (a, i, f) =>
                    {
                        int M = a + (i > 0 ? I[i - 1].Value : 0);
                        if (I[1].Value < 0)
                        {
                            J.Value = PC + 1;
                            PC = M;
                        }
                        else
                            PC++;
                    }),
                new MIXInstruction("J2Z",
                    (a, i, f) =>
                    {
                        int M = a + (i > 0 ? I[i - 1].Value : 0);
                        if (I[1].Value == 0)
                        {
                            J.Value = PC + 1;
                            PC = M;
                        }
                        else
                            PC++;
                    }),
                new MIXInstruction("J2P",
                    (a, i, f) =>
                    {
                        int M = a + (i > 0 ? I[i - 1].Value : 0);
                        if (I[1].Value > 0)
                        {
                            J.Value = PC + 1;
                            PC = M;
                        }
                        else
                            PC++;
                    }),
                new MIXInstruction("J2NN",
                    (a, i, f) =>
                    {
                        int M = a + (i > 0 ? I[i - 1].Value : 0);
                        if (I[1].Value >= 0)
                        {
                            J.Value = PC + 1;
                            PC = M;
                        }
                        else
                            PC++;
                    }),
                new MIXInstruction("J2NP",
                    (a, i, f) =>
                    {
                        int M = a + (i > 0 ? I[i - 1].Value : 0);
                        if (I[1].Value <= 0)
                        {
                            J.Value = PC + 1;
                            PC = M;
                        }
                        else
                            PC++;
                    }),
                new MIXInstruction("J2NZ",
                    (a, i, f) =>
                    {
                        int M = a + (i > 0 ? I[i - 1].Value : 0);
                        if (I[1].Value != 0)
                        {
                            J.Value = PC + 1;
                            PC = M;
                        }
                        else
                            PC++;
                    }),
                new MIXInstruction("J3N",
                    (a, i, f) =>
                    {
                        int M = a + (i > 0 ? I[i - 1].Value : 0);
                        if (I[2].Value < 0)
                        {
                            J.Value = PC + 1;
                            PC = M;
                        }
                        else
                            PC++;
                    }),
                new MIXInstruction("J3Z",
                    (a, i, f) =>
                    {
                        int M = a + (i > 0 ? I[i - 1].Value : 0);
                        if (I[2].Value == 0)
                        {
                            J.Value = PC + 1;
                            PC = M;
                        }
                        else
                            PC++;
                    }),
                new MIXInstruction("J3P",
                    (a, i, f) =>
                    {
                        int M = a + (i > 0 ? I[i - 1].Value : 0);
                        if (I[2].Value > 0)
                        {
                            J.Value = PC + 1;
                            PC = M;
                        }
                        else
                            PC++;
                    }),
                new MIXInstruction("J3NN",
                    (a, i, f) =>
                    {
                        int M = a + (i > 0 ? I[i - 1].Value : 0);
                        if (I[2].Value >= 0)
                        {
                            J.Value = PC + 1;
                            PC = M;
                        }
                        else
                            PC++;
                    }),
                new MIXInstruction("J3NP",
                    (a, i, f) =>
                    {
                        int M = a + (i > 0 ? I[i - 1].Value : 0);
                        if (I[2].Value <= 0)
                        {
                            J.Value = PC + 1;
                            PC = M;
                        }
                        else
                            PC++;
                    }),
                new MIXInstruction("J3NZ",
                    (a, i, f) =>
                    {
                        int M = a + (i > 0 ? I[i - 1].Value : 0);
                        if (I[2].Value != 0)
                        {
                            J.Value = PC + 1;
                            PC = M;
                        }
                        else
                            PC++;
                    }),
                new MIXInstruction("J4N",
                    (a, i, f) =>
                    {
                        int M = a + (i > 0 ? I[i - 1].Value : 0);
                        if (I[3].Value < 0)
                        {
                            J.Value = PC + 1;
                            PC = M;
                        }
                        else
                            PC++;
                    }),
                new MIXInstruction("J4Z",
                    (a, i, f) =>
                    {
                        int M = a + (i > 0 ? I[i - 1].Value : 0);
                        if (I[3].Value == 0)
                        {
                            J.Value = PC + 1;
                            PC = M;
                        }
                        else
                            PC++;
                    }),
                new MIXInstruction("J4P",
                    (a, i, f) =>
                    {
                        int M = a + (i > 0 ? I[i - 1].Value : 0);
                        if (I[3].Value > 0)
                        {
                            J.Value = PC + 1;
                            PC = M;
                        }
                        else
                            PC++;
                    }),
                new MIXInstruction("J4NN",
                    (a, i, f) =>
                    {
                        int M = a + (i > 0 ? I[i - 1].Value : 0);
                        if (I[3].Value >= 0)
                        {
                            J.Value = PC + 1;
                            PC = M;
                        }
                        else
                            PC++;
                    }),
                new MIXInstruction("J4NP",
                    (a, i, f) =>
                    {
                        int M = a + (i > 0 ? I[i - 1].Value : 0);
                        if (I[3].Value <= 0)
                        {
                            J.Value = PC + 1;
                            PC = M;
                        }
                        else
                            PC++;
                    }),
                new MIXInstruction("J4NZ",
                    (a, i, f) =>
                    {
                        int M = a + (i > 0 ? I[i - 1].Value : 0);
                        if (I[3].Value != 0)
                        {
                            J.Value = PC + 1;
                            PC = M;
                        }
                        else
                            PC++;
                    }),
                new MIXInstruction("J5N",
                    (a, i, f) =>
                    {
                        int M = a + (i > 0 ? I[i - 1].Value : 0);
                        if (I[4].Value < 0)
                        {
                            J.Value = PC + 1;
                            PC = M;
                        }
                        else
                            PC++;
                    }),
                new MIXInstruction("J5Z",
                    (a, i, f) =>
                    {
                        int M = a + (i > 0 ? I[i - 1].Value : 0);
                        if (I[4].Value == 0)
                        {
                            J.Value = PC + 1;
                            PC = M;
                        }
                        else
                            PC++;
                    }),
                new MIXInstruction("J5P",
                    (a, i, f) =>
                    {
                        int M = a + (i > 0 ? I[i - 1].Value : 0);
                        if (I[4].Value > 0)
                        {
                            J.Value = PC + 1;
                            PC = M;
                        }
                        else
                            PC++;
                    }),
                new MIXInstruction("J5NN",
                    (a, i, f) =>
                    {
                        int M = a + (i > 0 ? I[i - 1].Value : 0);
                        if (I[4].Value >= 0)
                        {
                            J.Value = PC + 1;
                            PC = M;
                        }
                        else
                            PC++;
                    }),
                new MIXInstruction("J5NP",
                    (a, i, f) =>
                    {
                        int M = a + (i > 0 ? I[i - 1].Value : 0);
                        if (I[4].Value <= 0)
                        {
                            J.Value = PC + 1;
                            PC = M;
                        }
                        else
                            PC++;
                    }),
                new MIXInstruction("J5NZ",
                    (a, i, f) =>
                    {
                        int M = a + (i > 0 ? I[i - 1].Value : 0);
                        if (I[4].Value != 0)
                        {
                            J.Value = PC + 1;
                            PC = M;
                        }
                        else
                            PC++;
                    }),
                new MIXInstruction("J6N",
                    (a, i, f) =>
                    {
                        int M = a + (i > 0 ? I[i - 1].Value : 0);
                        if (I[5].Value < 0)
                        {
                            J.Value = PC + 1;
                            PC = M;
                        }
                        else
                            PC++;
                    }),
                new MIXInstruction("J6Z",
                    (a, i, f) =>
                    {
                        int M = a + (i > 0 ? I[i - 1].Value : 0);
                        if (I[5].Value == 0)
                        {
                            J.Value = PC + 1;
                            PC = M;
                        }
                        else
                            PC++;
                    }),
                new MIXInstruction("J6P",
                    (a, i, f) =>
                    {
                        int M = a + (i > 0 ? I[i - 1].Value : 0);
                        if (I[5].Value > 0)
                        {
                            J.Value = PC + 1;
                            PC = M;
                        }
                        else
                            PC++;
                    }),
                new MIXInstruction("J6NN",
                    (a, i, f) =>
                    {
                        int M = a + (i > 0 ? I[i - 1].Value : 0);
                        if (I[5].Value >= 0)
                        {
                            J.Value = PC + 1;
                            PC = M;
                        }
                        else
                            PC++;
                    }),
                new MIXInstruction("J6NP",
                    (a, i, f) =>
                    {
                        int M = a + (i > 0 ? I[i - 1].Value : 0);
                        if (I[5].Value <= 0)
                        {
                            J.Value = PC + 1;
                            PC = M;
                        }
                        else
                            PC++;
                    }),
                new MIXInstruction("J6NZ",
                    (a, i, f) =>
                    {
                        int M = a + (i > 0 ? I[i - 1].Value : 0);
                        if (I[5].Value != 0)
                        {
                            J.Value = PC + 1;
                            PC = M;
                        }
                        else
                            PC++;
                    }),

                #endregion

                #region Miscellaneous

                new MIXInstruction("SLA",
                    (a, i, f) =>
                    {
                        int M = a + (i > 0 ? I[i - 1].Value : 0);

                        string result = Convert.ToString(A.UnsignedValue, 2).PadLeft(30, '0');
                        for (int k = 0; k < M; k++)
                            result += "000000";
                        result = result.Substring(result.Length - 30);

                        A.UnsignedValue = Convert.ToInt32(result, 2);

                        PC++;
                    }),
                new MIXInstruction("SRA",
                    (a, i, f) =>
                    {
                        int M = a + (i > 0 ? I[i - 1].Value : 0);

                        string result = Convert.ToString(A.UnsignedValue, 2).PadLeft(30, '0');
                        for (int k = 0; k < M; k++)
                            result = "000000" + result;
                        result = result.Substring(0, 30);

                        A.UnsignedValue = Convert.ToInt32(result, 2);

                        PC++;
                    }),
                new MIXInstruction("SLAX",
                    (a, i, f) =>
                    {
                        int M = a + (i > 0 ? I[i - 1].Value : 0);

                        string result = Convert.ToString(A.UnsignedValue, 2).PadLeft(30, '0') + Convert.ToString(X.UnsignedValue, 2).PadLeft(30, '0');
                        for (int k = 0; k < M; k++)
                            result += "000000";
                        result = result.Substring(result.Length - 60);

                        A.UnsignedValue = Convert.ToInt32(result.Substring(0, 30), 2);
                        X.UnsignedValue = Convert.ToInt32(result.Substring(30), 2);

                        PC++;
                    }),
                new MIXInstruction("SRAX",
                    (a, i, f) =>
                    {
                        int M = a + (i > 0 ? I[i - 1].Value : 0);

                        string result = Convert.ToString(A.UnsignedValue, 2).PadLeft(30, '0') + Convert.ToString(X.UnsignedValue, 2).PadLeft(30, '0');
                        for (int k = 0; k < M; k++)
                            result = "000000" + result;
                        result = result.Substring(0, 60);

                        A.UnsignedValue = Convert.ToInt32(result.Substring(0, 30), 2);
                        X.UnsignedValue = Convert.ToInt32(result.Substring(30), 2);

                        PC++;
                    }),
                new MIXInstruction("SLC",
                    (a, i, f) =>
                    {
                        int M = (a + (i > 0 ? I[i - 1].Value : 0)) % 10;

                        string result = Convert.ToString(A.UnsignedValue, 2).PadLeft(30, '0') + Convert.ToString(X.UnsignedValue, 2).PadLeft(30, '0');
                        result = result.Substring(M * 6) + result.Substring(0, M * 6);

                        A.UnsignedValue = Convert.ToInt32(result.Substring(0, 30), 2);
                        X.UnsignedValue = Convert.ToInt32(result.Substring(30), 2);

                        PC++;
                    }),
                new MIXInstruction("SRC",
                    (a, i, f) =>
                    {
                        int M = (a + (i > 0 ? I[i - 1].Value : 0)) % 10;

                        string result = Convert.ToString(A.UnsignedValue, 2).PadLeft(30, '0') + Convert.ToString(X.UnsignedValue, 2).PadLeft(30, '0');
                        result = result.Substring(60 - M * 6) + result.Substring(0, 60 - M * 6);

                        A.UnsignedValue = Convert.ToInt32(result.Substring(0, 30), 2);
                        X.UnsignedValue = Convert.ToInt32(result.Substring(30), 2);

                        PC++;
                    }),
                new MIXInstruction("MOVE",
                    (a, i, f) =>
                    {
                        int M = a + (i > 0 ? I[i - 1].Value : 0);
                        int target = I[0].Value;
                        for (int j = 0; j < f; j++, I[0].Value++)
                            Memory[target + j].Value = Memory[M + j].Value;

                        PC++;
                    }),
                new MIXInstruction("NOP",
                    (a, i, f) =>
                    {
                        PC++;
                    }),
                new MIXInstruction("HLT",
                    (a, i, f) =>
                    {
                        PC++;

                        Running = false;
                    }),

                #endregion

                #region Input/Output

                new MIXInstruction("IN",
                    (a, i, f) =>
                    {
                        if (devices[f] != null)
                        {
                            int M = a + (i > 0 ? I[i - 1].Value : 0);
                            devices[f].In(M);
                        }

                        PC++;
                    }),
                new MIXInstruction("OUT",
                    (a, i, f) =>
                    {
                        if (devices[f] != null)
                        {
                            int M = a + (i > 0 ? I[i - 1].Value : 0);
                            devices[f].Out(M);
                        }

                        PC++;
                    }),
                new MIXInstruction("IOC",
                    (a, i, f) =>
                    {
                        if (devices[f] != null)
                        {
                            int M = a + (i > 0 ? I[i - 1].Value : 0);
                            devices[f].IOC(M);
                        }

                        PC++;
                    }),
                new MIXInstruction("JRED",
                    (a, i, f) =>
                    {
                        if (devices[f] != null && devices[f].Ready)
                        {
                            int M = a + (i > 0 ? I[i - 1].Value : 0);
                            J.Value = PC + 1;
                            PC = M;
                        }
                        else
                            PC++;
                    }),
                new MIXInstruction("JBUS",
                    (a, i, f) =>
                    {
                        if (devices[f] != null && !devices[f].Ready)
                        {
                            int M = a + (i > 0 ? I[i - 1].Value : 0);
                            J.Value = PC + 1;
                            PC = M;
                        }
                        else
                            PC++;
                    }),

                #endregion

                #region Conversion

                new MIXInstruction("NUM",
                    (a, i, f) =>
                    {
                        string result = "";
                        result += A[1] % 10;
                        result += A[2] % 10;
                        result += A[3] % 10;
                        result += A[4] % 10;
                        result += A[5] % 10;
                        result += X[1] % 10;
                        result += X[2] % 10;
                        result += X[3] % 10;
                        result += X[4] % 10;
                        result += X[5] % 10;

                        long lVal = Convert.ToInt64(result);
                        if (lVal > MIXWord.MaxValue)
                            lVal %= 7776;

                        A.UnsignedValue = Convert.ToInt32(lVal);

                        PC++;
                    }),
                new MIXInstruction("CHAR",
                    (a, i, f) =>
                    {
                        string result = A.UnsignedValue.ToString().PadLeft(10, '0');
                        for (byte j = 0; j < 5; j++)
                        {
                            var b = MIXMachine.CHAR_TABLE[result[j]];
                            A[(byte)(j + 1)] = b;
                        }
                        for (byte j = 5; j < 10; j++)
                        {
                            var b = MIXMachine.CHAR_TABLE[result[j]];
                            X[(byte)(j - 4)] = b;
                        }

                        PC++;
                    }),

                #endregion
            };
        }

        #endregion

        #region Static Constructs

        static MIXMachine()
        {
            MakeCharTable();
            MakeInstructionList();
        }

        #region Character Table

        public static Dictionary<char, byte> CHAR_TABLE;

        private static void MakeCharTable()
        {
            CHAR_TABLE = new Dictionary<char, byte>()
            {
                { ' ', 0 },
                { 'A', 1 },
                { 'B', 2 },
                { 'C', 3 },
                { 'D', 4 },
                { 'E', 5 },
                { 'F', 6 },
                { 'G', 7 },
                { 'H', 8 },
                { 'I', 9 },
                { 'Δ', 10 },
                { 'J', 11 },
                { 'K', 12 },
                { 'L', 13 },
                { 'M', 14 },
                { 'N', 15 },
                { 'O', 16 },
                { 'P', 17 },
                { 'Q', 18 },
                { 'R', 19 },
                { 'Σ', 20 },
                { 'Π', 21 },
                { 'S', 22 },
                { 'T', 23 },
                { 'U', 24 },
                { 'V', 25 },
                { 'W', 26 },
                { 'X', 27 },
                { 'Y', 28 },
                { 'Z', 29 },

                { '0', 30 },
                { '1', 31 },
                { '2', 32 },
                { '3', 33 },
                { '4', 34 },
                { '5', 35 },
                { '6', 36 },
                { '7', 37 },
                { '8', 38 },
                { '9', 39 },

                { '.', 40 },
                { ',', 41 },
                { '(', 42 },
                { ')', 43 },
                { '+', 44 },
                { '-', 45 },
                { '*', 46 },
                { '/', 47 },
                { '=', 48 },
                { '$', 49 },
                { '<', 50 },
                { '>', 51 },
                { '@', 52 },
                { ';', 53 },
                { ':', 54 },
                { '\'', 55 },
            };
        }

        #endregion

        #region Instruction Table

        public static List<InstructionInfo> INSTRUCTION_LIST;

        static void MakeInstructionList()
        {
            INSTRUCTION_LIST = new List<InstructionInfo>()
            {
                new InstructionInfo { Name = "NOP", DefaultField = 0, OpCode = 0, Time = 1 },
                new InstructionInfo { Name = "ADD", DefaultField = 5, OpCode = 1, Time = 2 },
                new InstructionInfo { Name = "SUB", DefaultField = 5, OpCode = 2, Time = 2 },
                new InstructionInfo { Name = "MUL", DefaultField = 5, OpCode = 3, Time = 10 },
                new InstructionInfo { Name = "DIV", DefaultField = 5, OpCode = 4, Time = 12 },

                new InstructionInfo { Name = "NUM", DefaultField = 0, OpCode = 5, Time = 10 },
                new InstructionInfo { Name = "CHAR", DefaultField = 1, OpCode = 5, Time = 10 },
                new InstructionInfo { Name = "HLT", DefaultField = 2, OpCode = 5, Time = 10 },

                new InstructionInfo { Name = "SLA", DefaultField = 0, OpCode = 6, Time = 2 },
                new InstructionInfo { Name = "SRA", DefaultField = 1, OpCode = 6, Time = 2 },
                new InstructionInfo { Name = "SLAX", DefaultField = 2, OpCode = 6, Time = 2 },
                new InstructionInfo { Name = "SRAX", DefaultField = 3, OpCode = 6, Time = 2 },
                new InstructionInfo { Name = "SLC", DefaultField = 4, OpCode = 6, Time = 2 },
                new InstructionInfo { Name = "SRC", DefaultField = 5, OpCode = 6, Time = 2 },

                new InstructionInfo { Name = "MOVE", DefaultField = 1, OpCode = 7, Time = 1 },

                new InstructionInfo { Name = "LDA", DefaultField = 5, OpCode = 8, Time = 2 },
                new InstructionInfo { Name = "LD1", DefaultField = 5, OpCode = 9, Time = 2 },
                new InstructionInfo { Name = "LD2", DefaultField = 5, OpCode = 10, Time = 2 },
                new InstructionInfo { Name = "LD3", DefaultField = 5, OpCode = 11, Time = 2 },
                new InstructionInfo { Name = "LD4", DefaultField = 5, OpCode = 12, Time = 2 },
                new InstructionInfo { Name = "LD5", DefaultField = 5, OpCode = 13, Time = 2 },
                new InstructionInfo { Name = "LD6", DefaultField = 5, OpCode = 14, Time = 2 },
                new InstructionInfo { Name = "LDX", DefaultField = 5, OpCode = 15, Time = 2 },

                new InstructionInfo { Name = "LDAN", DefaultField = 5, OpCode = 16, Time = 2 },
                new InstructionInfo { Name = "LD1N", DefaultField = 5, OpCode = 17, Time = 2 },
                new InstructionInfo { Name = "LD2N", DefaultField = 5, OpCode = 18, Time = 2 },
                new InstructionInfo { Name = "LD3N", DefaultField = 5, OpCode = 19, Time = 2 },
                new InstructionInfo { Name = "LD4N", DefaultField = 5, OpCode = 20, Time = 2 },
                new InstructionInfo { Name = "LD5N", DefaultField = 5, OpCode = 21, Time = 2 },
                new InstructionInfo { Name = "LD6N", DefaultField = 5, OpCode = 22, Time = 2 },
                new InstructionInfo { Name = "LDXN", DefaultField = 5, OpCode = 23, Time = 2 },

                new InstructionInfo { Name = "STA", DefaultField = 5, OpCode = 24, Time = 2 },
                new InstructionInfo { Name = "ST1", DefaultField = 5, OpCode = 25, Time = 2 },
                new InstructionInfo { Name = "ST2", DefaultField = 5, OpCode = 26, Time = 2 },
                new InstructionInfo { Name = "ST3", DefaultField = 5, OpCode = 27, Time = 2 },
                new InstructionInfo { Name = "ST4", DefaultField = 5, OpCode = 28, Time = 2 },
                new InstructionInfo { Name = "ST5", DefaultField = 5, OpCode = 29, Time = 2 },
                new InstructionInfo { Name = "ST6", DefaultField = 5, OpCode = 30, Time = 2 },
                new InstructionInfo { Name = "STX", DefaultField = 5, OpCode = 31, Time = 2 },
                new InstructionInfo { Name = "STJ", DefaultField = 2, OpCode = 32, Time = 2 },
                new InstructionInfo { Name = "STZ", DefaultField = 5, OpCode = 33, Time = 2 },

                new InstructionInfo { Name = "JBUS", DefaultField = 0, OpCode = 34, Time = 1 },
                new InstructionInfo { Name = "IOC", DefaultField = 0, OpCode = 35, Time = 1 },
                new InstructionInfo { Name = "IN", DefaultField = 0, OpCode = 36, Time = 1 },
                new InstructionInfo { Name = "OUT", DefaultField = 0, OpCode = 37, Time = 1 },
                new InstructionInfo { Name = "JRED", DefaultField = 0, OpCode = 38, Time = 1 },

                new InstructionInfo { Name = "JMP", DefaultField = 0, OpCode = 39, Time = 1 },
                new InstructionInfo { Name = "JSJ", DefaultField = 1, OpCode = 39, Time = 1 },
                new InstructionInfo { Name = "JOV", DefaultField = 2, OpCode = 39, Time = 1 },
                new InstructionInfo { Name = "JNOV", DefaultField = 3, OpCode = 39, Time = 1 },
                new InstructionInfo { Name = "JL", DefaultField = 4, OpCode = 39, Time = 1 },
                new InstructionInfo { Name = "JE", DefaultField = 5, OpCode = 39, Time = 1 },
                new InstructionInfo { Name = "JG", DefaultField = 6, OpCode = 39, Time = 1 },
                new InstructionInfo { Name = "JGE", DefaultField = 7, OpCode = 39, Time = 1 },
                new InstructionInfo { Name = "JNE", DefaultField = 8, OpCode = 39, Time = 1 },
                new InstructionInfo { Name = "JLE", DefaultField = 9, OpCode = 39, Time = 1 },

                new InstructionInfo { Name = "JAN", DefaultField = 0, OpCode = 40, Time = 1 },
                new InstructionInfo { Name = "JAZ", DefaultField = 1, OpCode = 40, Time = 1 },
                new InstructionInfo { Name = "JAP", DefaultField = 2, OpCode = 40, Time = 1 },
                new InstructionInfo { Name = "JANN", DefaultField = 3, OpCode = 40, Time = 1 },
                new InstructionInfo { Name = "JANZ", DefaultField = 4, OpCode = 40, Time = 1 },
                new InstructionInfo { Name = "JANP", DefaultField = 5, OpCode = 40, Time = 1 },

                new InstructionInfo { Name = "J1N", DefaultField = 0, OpCode = 41, Time = 1 },
                new InstructionInfo { Name = "J1Z", DefaultField = 1, OpCode = 41, Time = 1 },
                new InstructionInfo { Name = "J1P", DefaultField = 2, OpCode = 41, Time = 1 },
                new InstructionInfo { Name = "J1NN", DefaultField = 3, OpCode = 41, Time = 1 },
                new InstructionInfo { Name = "J1NZ", DefaultField = 4, OpCode = 41, Time = 1 },
                new InstructionInfo { Name = "J1NP", DefaultField = 5, OpCode = 41, Time = 1 },

                new InstructionInfo { Name = "J2N", DefaultField = 0, OpCode = 42, Time = 1 },
                new InstructionInfo { Name = "J2Z", DefaultField = 1, OpCode = 42, Time = 1 },
                new InstructionInfo { Name = "J2P", DefaultField = 2, OpCode = 42, Time = 1 },
                new InstructionInfo { Name = "J2NN", DefaultField = 3, OpCode = 42, Time = 1 },
                new InstructionInfo { Name = "J2NZ", DefaultField = 4, OpCode = 42, Time = 1 },
                new InstructionInfo { Name = "J2NP", DefaultField = 5, OpCode = 42, Time = 1 },

                new InstructionInfo { Name = "J3N", DefaultField = 0, OpCode = 43, Time = 1 },
                new InstructionInfo { Name = "J3Z", DefaultField = 1, OpCode = 43, Time = 1 },
                new InstructionInfo { Name = "J3P", DefaultField = 2, OpCode = 43, Time = 1 },
                new InstructionInfo { Name = "J3NN", DefaultField = 3, OpCode = 43, Time = 1 },
                new InstructionInfo { Name = "J3NZ", DefaultField = 4, OpCode = 43, Time = 1 },
                new InstructionInfo { Name = "J3NP", DefaultField = 5, OpCode = 43, Time = 1 },

                new InstructionInfo { Name = "J4N", DefaultField = 0, OpCode = 44, Time = 1 },
                new InstructionInfo { Name = "J4Z", DefaultField = 1, OpCode = 44, Time = 1 },
                new InstructionInfo { Name = "J4P", DefaultField = 2, OpCode = 44, Time = 1 },
                new InstructionInfo { Name = "J4NN", DefaultField = 3, OpCode = 44, Time = 1 },
                new InstructionInfo { Name = "J4NZ", DefaultField = 4, OpCode = 44, Time = 1 },
                new InstructionInfo { Name = "J4NP", DefaultField = 5, OpCode = 44, Time = 1 },

                new InstructionInfo { Name = "J5N", DefaultField = 0, OpCode = 45, Time = 1 },
                new InstructionInfo { Name = "J5Z", DefaultField = 1, OpCode = 45, Time = 1 },
                new InstructionInfo { Name = "J5P", DefaultField = 2, OpCode = 45, Time = 1 },
                new InstructionInfo { Name = "J5NN", DefaultField = 3, OpCode = 45, Time = 1 },
                new InstructionInfo { Name = "J5NZ", DefaultField = 4, OpCode = 45, Time = 1 },
                new InstructionInfo { Name = "J5NP", DefaultField = 5, OpCode = 45, Time = 1 },

                new InstructionInfo { Name = "J6N", DefaultField = 0, OpCode = 46, Time = 1 },
                new InstructionInfo { Name = "J6Z", DefaultField = 1, OpCode = 46, Time = 1 },
                new InstructionInfo { Name = "J6P", DefaultField = 2, OpCode = 46, Time = 1 },
                new InstructionInfo { Name = "J6NN", DefaultField = 3, OpCode = 46, Time = 1 },
                new InstructionInfo { Name = "J6NZ", DefaultField = 4, OpCode = 46, Time = 1 },
                new InstructionInfo { Name = "J6NP", DefaultField = 5, OpCode = 46, Time = 1 },

                new InstructionInfo { Name = "JXN", DefaultField = 0, OpCode = 47, Time = 1 },
                new InstructionInfo { Name = "JXZ", DefaultField = 1, OpCode = 47, Time = 1 },
                new InstructionInfo { Name = "JXP", DefaultField = 2, OpCode = 47, Time = 1 },
                new InstructionInfo { Name = "JXNN", DefaultField = 3, OpCode = 47, Time = 1 },
                new InstructionInfo { Name = "JXNZ", DefaultField = 4, OpCode = 47, Time = 1 },
                new InstructionInfo { Name = "JXNP", DefaultField = 5, OpCode = 47, Time = 1 },

                new InstructionInfo { Name = "INCA", DefaultField = 0, OpCode = 48, Time = 1 },
                new InstructionInfo { Name = "DECA", DefaultField = 1, OpCode = 48, Time = 1 },
                new InstructionInfo { Name = "ENTA", DefaultField = 2, OpCode = 48, Time = 1 },
                new InstructionInfo { Name = "ENNA", DefaultField = 3, OpCode = 48, Time = 1 },

                new InstructionInfo { Name = "INC1", DefaultField = 0, OpCode = 49, Time = 1 },
                new InstructionInfo { Name = "DEC1", DefaultField = 1, OpCode = 49, Time = 1 },
                new InstructionInfo { Name = "ENT1", DefaultField = 2, OpCode = 49, Time = 1 },
                new InstructionInfo { Name = "ENN1", DefaultField = 3, OpCode = 49, Time = 1 },

                new InstructionInfo { Name = "INC2", DefaultField = 0, OpCode = 50, Time = 1 },
                new InstructionInfo { Name = "DEC2", DefaultField = 1, OpCode = 50, Time = 1 },
                new InstructionInfo { Name = "ENT2", DefaultField = 2, OpCode = 50, Time = 1 },
                new InstructionInfo { Name = "ENN2", DefaultField = 3, OpCode = 50, Time = 1 },

                new InstructionInfo { Name = "INC3", DefaultField = 0, OpCode = 51, Time = 1 },
                new InstructionInfo { Name = "DEC3", DefaultField = 1, OpCode = 51, Time = 1 },
                new InstructionInfo { Name = "ENT3", DefaultField = 2, OpCode = 51, Time = 1 },
                new InstructionInfo { Name = "ENN3", DefaultField = 3, OpCode = 51, Time = 1 },

                new InstructionInfo { Name = "INC4", DefaultField = 0, OpCode = 52, Time = 1 },
                new InstructionInfo { Name = "DEC4", DefaultField = 1, OpCode = 52, Time = 1 },
                new InstructionInfo { Name = "ENT4", DefaultField = 2, OpCode = 52, Time = 1 },
                new InstructionInfo { Name = "ENN4", DefaultField = 3, OpCode = 52, Time = 1 },

                new InstructionInfo { Name = "INC5", DefaultField = 0, OpCode = 53, Time = 1 },
                new InstructionInfo { Name = "DEC5", DefaultField = 1, OpCode = 53, Time = 1 },
                new InstructionInfo { Name = "ENT5", DefaultField = 2, OpCode = 53, Time = 1 },
                new InstructionInfo { Name = "ENN5", DefaultField = 3, OpCode = 53, Time = 1 },

                new InstructionInfo { Name = "INC6", DefaultField = 0, OpCode = 54, Time = 1 },
                new InstructionInfo { Name = "DEC6", DefaultField = 1, OpCode = 54, Time = 1 },
                new InstructionInfo { Name = "ENT6", DefaultField = 2, OpCode = 54, Time = 1 },
                new InstructionInfo { Name = "ENN6", DefaultField = 3, OpCode = 54, Time = 1 },

                new InstructionInfo { Name = "INCX", DefaultField = 0, OpCode = 55, Time = 1 },
                new InstructionInfo { Name = "DECX", DefaultField = 1, OpCode = 55, Time = 1 },
                new InstructionInfo { Name = "ENTX", DefaultField = 2, OpCode = 55, Time = 1 },
                new InstructionInfo { Name = "ENNX", DefaultField = 3, OpCode = 55, Time = 1 },

                new InstructionInfo { Name = "CMPA", DefaultField = 5, OpCode = 56, Time = 2 },
                new InstructionInfo { Name = "CMP1", DefaultField = 5, OpCode = 57, Time = 2 },
                new InstructionInfo { Name = "CMP2", DefaultField = 5, OpCode = 58, Time = 2 },
                new InstructionInfo { Name = "CMP3", DefaultField = 5, OpCode = 59, Time = 2 },
                new InstructionInfo { Name = "CMP4", DefaultField = 5, OpCode = 60, Time = 2 },
                new InstructionInfo { Name = "CMP5", DefaultField = 5, OpCode = 61, Time = 2 },
                new InstructionInfo { Name = "CMP6", DefaultField = 5, OpCode = 62, Time = 2 },
                new InstructionInfo { Name = "CMPX", DefaultField = 5, OpCode = 63, Time = 2 },
            };
        }

        #endregion

        #endregion
    }
}