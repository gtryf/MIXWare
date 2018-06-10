using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MIXLib
{
    #region Abstract Device

    public abstract class MIXDevice
    {
        /// <summary>
        /// The descriptive name of this device
        /// </summary>
        public string Name { get; private set; }
        /// <summary>
        /// The device's backing store
        /// </summary>
        protected Stream Store 
        {
			get => store;
            set 
            {
                lock (this)
                {
                    Ready = false;
                    store = value;
                    OnStoreChanged();
                    Ready = true;
                }
            }
        }

        public bool Ready { get; protected set; }

        private Stream store;

        protected virtual void OnStoreChanged() { }

        /// <summary>
        /// The device's block size in MIX words
        /// </summary>
        protected byte BlockSize { get; private set; }
        /// <summary>
        /// The MIX machine this device is attached to
        /// </summary>
        protected MIXMachine Machine { get; private set; }
        
        protected MIXDevice(MIXMachine machine, string name, Stream store, byte blockSize)
        {
            Name = name;
            Store = store;
            BlockSize = blockSize;
            Machine = machine;
        }

        public void Redirect(Stream newStore)
        {
            if (Store != null) Store.Close();
            Store = newStore;
        }

        public void Flush()
        {
            if (Store != null && Store.CanWrite) Store.Flush();
        }

        protected string MIXWordToString(MIXWord w)
        {
            string result = "";

            for (byte i = 1; i < 6; i++)
            {
                var ch = from c in MIXMachine.CHAR_TABLE
                         where w[i] == c.Value
                         select c.Key;
                result += ch.DefaultIfEmpty('█').First().ToString();
            }

            return result;
        }

        public override string ToString()
        {
            string result = "";
            if (Store == null)
                result = string.Format("NAME: {0}; BLOCK SIZE: {1}; BACKING STORE: N/A", Name, BlockSize);
            else if (Store is FileStream)
                result = string.Format("NAME: {0}, BLOCK SIZE: {1}, BACKING STORE: {2}", Name, BlockSize, (Store as FileStream).Name);
            else if (Store is MemoryStream)
                result = string.Format("NAME: {0}, BLOCK SIZE: {1}, BACKING STORE: MEMORY", Name, BlockSize);
            else
                result = string.Format("NAME: {0}, BLOCK SIZE: {1}, BACKING STORE: CONSOLE", Name, BlockSize);

            return string.Format(result);
        }

        public Task Out(int M) => Task.Run(() => OutProc(M));

        public Task In(int M) => Task.Run(() => InProc(M));

        public Task IOC(int M) => Task.Run(() => IOC(M));

        protected abstract void OutProc(object M);
        protected abstract void InProc(object M);
        protected abstract void IOCProc(object M);
    }

    #endregion

    #region Tapes

    public class Tape : MIXDevice
    {
        public Tape(MIXMachine machine, Stream store) : base(machine, "TAPE", store, 100) { }

        protected override void OutProc(object data)
        {
            SpinWait.SpinUntil(() => Ready);

            Ready = false;

            if (Store != null)
            {
                int M = (int)data;

                for (int i = 0; i < BlockSize; i++)
                {
                    byte[] buffer = Machine.Memory[M + i].ToByteArray();
                    foreach (var b in buffer)
                        Store.WriteByte(b);
                }
            }

            Ready = true;
        }

        protected override void InProc(object data)
        {
            SpinWait.SpinUntil(() => Ready);

            Ready = false;

            if (Store != null)
            {
                int M = (int)data;

                for (int i = 0; i < BlockSize; i++)
                {
                    byte[] buffer = new byte[6];
                    using (MemoryStream ms = new MemoryStream())
                    {
                        int read;
                        read = Store.Read(buffer, 0, 6);
                        while (read > 0)
                        {
                            ms.Write(buffer, 0, read);
                            read = Store.Read(buffer, 0, 6);
                        }
                        MIXWord w = MIXWord.FromByteArray(ms.ToArray());
                        Machine.Memory[M + i].Value = w.Value;
                    }
                }
            }

            Ready = true;
        }

        protected override void IOCProc(object data)
        {
            SpinWait.SpinUntil(() => Ready);

            Ready = false;

            if (Store != null)
            {
                int M = (int)data;
                if (M == 0)
                    Store.Seek(0, SeekOrigin.Begin);
                else if (M < 0)
                    Store.Seek(-(M * BlockSize * 6), SeekOrigin.Current);
                else
                    Store.Seek(M * BlockSize * 6, SeekOrigin.Current);
            }

            Ready = true;
        }
    }

    #endregion

    #region Disks

    public class Disk : MIXDevice
    {
        public Disk(MIXMachine machine, Stream store) : base(machine, "DISK", store, 100) { }

        protected override void OutProc(object data)
        {
            SpinWait.SpinUntil(() => Ready);

            Ready = false;

            if (Store != null)
            {
                int M = (int)data;
                Store.Seek(Machine.X * BlockSize, SeekOrigin.Begin);

                for (int i = 0; i < BlockSize; i++)
                {
                    byte[] buffer = Machine.Memory[M + i].ToByteArray();
                    foreach (var b in buffer)
                        Store.WriteByte(b);
                }
            }

            Ready = true;
        }

        protected override void InProc(object data)
        {
            SpinWait.SpinUntil(() => Ready);

            Ready = false;

            if (Store != null)
            {
                int M = (int)data;
                Store.Seek(Machine.X * BlockSize, SeekOrigin.Begin);

                for (int i = 0; i < BlockSize; i++)
                {
                    byte[] buffer = new byte[6];
                    using (MemoryStream ms = new MemoryStream())
                    {
                        int read;
                        read = Store.Read(buffer, 0, 6);
                        while (read > 0)
                        {
                            ms.Write(buffer, 0, read);
                            read = Store.Read(buffer, 0, 6);
                        }
                        MIXWord w = MIXWord.FromByteArray(ms.ToArray());
                        Machine.Memory[M + i].Value = w.Value;
                    }
                }
            }

            Ready = true;
        }

        protected override void IOCProc(object data)
        {
            SpinWait.SpinUntil(() => Ready);

            Ready = false;

            if (Store != null)
            {
                int M = (int)data;
                if (M == 0)
                    Store.Seek(Machine.X * BlockSize, SeekOrigin.Begin);
            }

            Ready = true;
        }
    }

    #endregion

    #region Card Reader

    public class CardReader : MIXDevice
    {
        public CardReader(MIXMachine machine, Stream store) : base(machine, "CARD_READER", store, 16) { }

        protected override void OutProc(object data)
        {
        }

        protected StreamReader storeReader;
        protected override void OnStoreChanged()
        {
            if (storeReader != null) storeReader.Close();
            if (Store != null)
                storeReader = new StreamReader(Store, true);
            else
                storeReader = null;
        }

        protected override void InProc(object data)
        {
            SpinWait.SpinUntil(() => Ready);

            Ready = false;

            if (Store != null)
            {
                int M = (int)data;

                if (storeReader.Peek() != -1)
                {
                    string inp = storeReader.ReadLine().PadRight(BlockSize * 5);

                    // Break the input string into chunks of 5.
                    // Should be BlockSize chunks.
                    var chunks = inp
                        .Select((x, i) => new { Index = i, Value = x })
                        .GroupBy(x => x.Index / 5)
                        .Select(x => x.Select(v => v.Value).ToList())
                        .ToList();

                    int j = 0;
                    foreach (var curr in chunks)
                    {
                        MIXWord w = new MIXWord();
                        w[1] = MIXMachine.CHAR_TABLE[curr[0]];
                        w[2] = MIXMachine.CHAR_TABLE[curr[1]];
                        w[3] = MIXMachine.CHAR_TABLE[curr[2]];
                        w[4] = MIXMachine.CHAR_TABLE[curr[3]];
                        w[5] = MIXMachine.CHAR_TABLE[curr[4]];

                        Machine.Memory[M + j] = w;
                        j++;
                    }
                }
            }

            Ready = true;
        }

        protected override void IOCProc(object data)
        {
        }
    }

    #endregion

    #region Card Punch

    public class CardPunch : MIXDevice
    {
        public CardPunch(MIXMachine machine, Stream store) : base(machine, "CARD_PUNCH", store, 16) { }

        protected override void OutProc(object data)
        {
            SpinWait.SpinUntil(() => Ready);

            Ready = false;

            if (Store != null)
            {
                int M = (int)data;
                Store.Seek(0, SeekOrigin.End);

                for (int j = 0; j < BlockSize; j++)
                {
                    string outp = MIXWordToString(Machine.Memory[M + j]);
                    storeWriter.Write(outp);
                }
                storeWriter.WriteLine();
            }

            Ready = true;
        }

        protected StreamWriter storeWriter;
        protected override void OnStoreChanged()
        {
            if (storeWriter != null) storeWriter.Close();
            if (Store != null)
            {
                storeWriter = new StreamWriter(Store);
                storeWriter.AutoFlush = true;
            }
            else
                storeWriter = null;
        }

        protected override void InProc(object data)
        {
        }

        protected override void IOCProc(object data)
        {
        }
    }

    #endregion

    #region Line Printer

    public class LinePrinter : MIXDevice
    {
        public LinePrinter(MIXMachine machine, Stream store) : base(machine, "LINE_PRINTER", store, 24) { }

        protected override void OutProc(object data)
        {
            SpinWait.SpinUntil(() => Ready);

            Ready = false;

            if (Store != null)
            {
                int M = (int)data;

                StreamWriter writer = new StreamWriter(Store);
                for (int j = 0; j < BlockSize; j++)
                {
                    string outp = MIXWordToString(Machine.Memory[M + j]);
                    writer.Write(outp);
                }
                writer.WriteLine();
                writer.Flush();
            }

            Ready = true;
        }

        protected override void InProc(object M)
        {
        }

        protected override void IOCProc(object data)
        {
            SpinWait.SpinUntil(() => Ready);

            Ready = false;

            if (Store != null)
            {
                int M = (int)data;
                if (M == 0)
                {
                    StreamWriter writer = new StreamWriter(Store);
                    writer.WriteLine("============ PAGE BREAK ============");
                    writer.Flush();
                }
            }

            Ready = true;
        }
    }

    #endregion

    #region Terminal

    public class Terminal
        : MIXDevice
    {
        public Terminal(MIXMachine machine, Stream store) : base(machine, "TERMINAL", store, 14) { }

        protected override void OutProc(object data)
        {
            SpinWait.SpinUntil(() => Ready);

            Ready = false;

            int M = (int)data;

            StreamWriter writer = new StreamWriter(Console.OpenStandardOutput());
            for (int j = 0; j < BlockSize; j++)
            {
                string outp = MIXWordToString(Machine.Memory[M + j]);
                writer.Write(outp);
            }
            writer.WriteLine();
            writer.Flush();

            Ready = true;
        }

        protected override void InProc(object data)
        {
            SpinWait.SpinUntil(() => Ready);

            Ready = false;

            int M = (int)data;

            StreamReader storeReader = new StreamReader(Console.OpenStandardInput());
            if (storeReader.Peek() != -1)
            {
                string inp = storeReader.ReadLine().PadRight(BlockSize * 5).Substring(0, BlockSize * 5);

                // Break the input string into chunks of 5.
                // Should be BlockSize chunks.
                var chunks = inp
                    .Select((x, i) => new { Index = i, Value = x })
                    .GroupBy(x => x.Index / 5)
                    .Select(x => x.Select(v => v.Value).ToList())
                    .ToList();

                int j = 0;
                foreach (var curr in chunks)
                {
                    MIXWord w = new MIXWord();
                    w[1] = MIXMachine.CHAR_TABLE[curr[0]];
                    w[2] = MIXMachine.CHAR_TABLE[curr[1]];
                    w[3] = MIXMachine.CHAR_TABLE[curr[2]];
                    w[4] = MIXMachine.CHAR_TABLE[curr[3]];
                    w[5] = MIXMachine.CHAR_TABLE[curr[4]];

                    Machine.Memory[M + j] = w;
                    j++;
                }
            }

            Ready = true;
        }

        protected override void IOCProc(object data)
        {
        }
    }

    #endregion

    #region Paper Tape

    public class PaperTape
        : MIXDevice
    {
        public PaperTape(MIXMachine machine, Stream store) : base(machine, "PAPER_TAPE", store, 14) { }

        protected override void OutProc(object data)
        {
            SpinWait.SpinUntil(() => Ready);

            Ready = false;

            int M = (int)data;

            StreamWriter writer = new StreamWriter(Store);
            for (int j = 0; j < BlockSize; j++)
            {
                string outp = MIXWordToString(Machine.Memory[M + j]);
                writer.Write(outp);
            }
            writer.WriteLine();
            writer.Flush();

            Ready = true;
        }

        protected override void InProc(object data)
        {
            SpinWait.SpinUntil(() => Ready);

            Ready = false;

            int M = (int)data;

            StreamReader storeReader = new StreamReader(Store);
            if (storeReader.Peek() != -1)
            {
                string inp = storeReader.ReadLine().PadRight(BlockSize * 5).Substring(0, BlockSize * 5);

                // Break the input string into chunks of 5.
                // Should be BlockSize chunks.
                var chunks = inp
                    .Select((x, i) => new { Index = i, Value = x })
                    .GroupBy(x => x.Index / 5)
                    .Select(x => x.Select(v => v.Value).ToList())
                    .ToList();

                int j = 0;
                foreach (var curr in chunks)
                {
                    MIXWord w = new MIXWord();
                    w[1] = MIXMachine.CHAR_TABLE[curr[0]];
                    w[2] = MIXMachine.CHAR_TABLE[curr[1]];
                    w[3] = MIXMachine.CHAR_TABLE[curr[2]];
                    w[4] = MIXMachine.CHAR_TABLE[curr[3]];
                    w[5] = MIXMachine.CHAR_TABLE[curr[4]];

                    Machine.Memory[M + j] = w;
                    j++;
                }
            }

            Ready = true;
        }

        protected override void IOCProc(object data)
        {
            SpinWait.SpinUntil(() => Ready);

            Ready = false;

            if (Store != null)
            {
                int M = (int)data;
                if (M == 0)
                    Store.Seek(0, SeekOrigin.Begin);
            }

            Ready = true;
        }
    }

    #endregion
}