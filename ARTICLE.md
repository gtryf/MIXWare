# A Simulator for Knuth's MIX Computer

## Introduction and Background

The MIX computer is an imaginary computer invented by Don Knuth in the 1960s in order to present his exposition on computer algorithms. As Knuth puts it, the use of an imaginary computer and machine language helps avoid distracting the reader with the technicalities of one particular computer system, and the focus remains on truths that have always been-and will always be-valid, independent of any kind of technological evolution or current trends.

There is no doubt about the truth of this statement. However, another kind of problem presents itself now. The MIX computer is, well, ...imaginary. A reader cannot experiment with it, or even have a go at solving the exercises by sitting in front of a real computer terminal and writing programs. Nor even can one be certain that a particular solution to an exercise is correct, unless one checks the answer to the exercise, and even then, there may be many different solutions to a problem, but the answers present only one. Unless of course the reader is inclined to simulate MIX in his or her head, or on a piece of paper. And these may very well have been Knuth's intentions for his readers. After all, there is no doubt that the reader that will get the most out of the books is the one who is patient and diligent enough to go through this kind of process. But it still is a daunting task, often too frustrating, and for many people in many ways, a distraction of the kind that Knuth wanted for his readers to avoid in the first place.

This is exactly the problem that the MIX simulator attempts to solve: a tool suite, capable of assembling and executing programs written in the MIXAL language, the MIX computer's assembly language. Features include a simplistic debugger, a complete implementation of the MIX computer's devices, symbol table generation, listing generation and typesetting for TeX, etc.

The full specification of the MIX computer and the MIXAL assembly language can be found in The Art of Computer Programming vol. 1. In fact, this software package has little use to anybody who has not read, or has no intention to read, this series of books, follow its examples, and solve its exercises. So, have fun using it, but first of all, enjoy reading the books!

## Using the Tools

The **MIXWare** distribution contains the complete documentation in the form of a *.pdf* file. In it, you may find the detailed instructions on how to execute the assembler and make the most out of the simulator. However, a (very) brief tutorial follows.

To assemble a MIXAL program:

```sh
MIXASM -f:card -o:primes.deck primes.mixal
```

This command assembles the file "*primes.mixal*" as a punched card deck and saves the output to "*primes.deck*".

The general syntax for MIXASM is:

```sh
MIXASM [arguments] [input-file]
```

where the optional `input-file` parameter is the name of a text file containing MIXAL code (if unspecified, input comes from the keyboard), and the `arguments` govern the compilation process. In particular, the `--output` (or `-o` for short) argument specifies the compiled output filename (if unspecified, it is sent to the standard output), for example *--output:primes.deck* sends the output to the file *primes.deck*. The `--format` (or `-f` for short) argument specifies whether the compiled output will be in the form of a punched card deck or a serialized binary file. For instance, `-f:card` specifies that the output is a card deck, while `-f:binary` specifies that the output is a binary serialized file. `--symtab` (or `-s` for short) outputs the program's symbol table, either to the standard output or to a specified file (works just like the `--output` argument). Similarly, `--list-file` (or `-lf` for short) produces a program listing. In this case, the filename is mandatory. In the case that you requested a listing or a symbol table, you may additionally specify `--pretty-print` (or `-pp` for short) to request that the output be a TeX file so that you may run it through TeX to be typeset nicely.

To start the simulator:

```sh
MIX --deck primes.deck or MIX --binary primes.bin
```

This command starts the simulator and loads the card deck found in the file "*primes.deck*" or "*primes.bin*", depending on whether the source has been compiled to a card deck or to a binary serialized file.

As soon as the simulator starts, it displays a prompt for you to input your commands. At this point, you may press `?` to get help on additional commands, the most important being "`run`", which begins execution of the program until it halts; "`step`", which executes only the next command pointed to by the Program Counter; and of course, "`quit`", if you've had enough of this! There exist a host of commands, all of which begin with "`show`", that echo the MIX computer's current state, as well as another set of commands, beginning with "`set`", that alter that state. Some examples are:

- `show memory` - shows the contents of MIX's memory. You may specify a range, such as in `show memory 3000 3010` to show only 11 words. If you specify a range, you may also request a disassembly, such as in `show memory 3000 3010 with disassembly`.
- `show state` - shows MIX's internal state, that is, the values of all the registers, flags, and the Program Counter.
- `set <reg> <value>` - sets the value of a particular MIX register, for example, `set rA 100`.

Chapters 1 and 2 of the documentation describe MIXASM and MIX in detail, including the simulator's command set and a full description of the assembler's command line arguments.

## Points of Interest

Developing these two tools has been a great exercise in compiler writing. The MIXAL compiler is written completely by hand. One particularly interesting point is that the MIXAL grammar had to be transformed to LL(1) in order for a recursive descent parser to be able to parse it. Chapter 4 of the documentation describes the development of the parser in some detail. Appendix B has the MIXAL syntax diagrams.

### Scanner Implementation

One interesting point to note is the scanner. The single most important property of the `Scanner` class is the `Tokens` property, an enumeration of `Token` objects. The `Token` class is not listed below, but in short, it contains the token type that the scanner just recognized, as well as its location in the source code file.

```csharp
public IEnumerable<Token> Tokens
{
    get
    {
        StringReader input = new StringReader(line);
        ScannerState state = ScannerState.LABEL;
        byte col = 0, endCol = 0;

        while (input.Peek() != -1)
        {
            char ch = (char)input.Peek();

            if (char.IsUpper(ch) || char.IsDigit(ch))
            {
                StringBuilder accum = new StringBuilder();
                while (char.IsUpper(ch) || char.IsDigit(ch))
                {
                    accum.Append(ch);
                    input.Read();
                    endCol++;

                    ch = (char)input.Peek(); // Get the next character
                }

                int temp = 0;
                switch (state)
                {
                    case ScannerState.LABEL:
                        // Labels cannot be numeric constants
                        if (int.TryParse(accum.ToString(), out temp))
                        {
                            throw new ScannerException(lineNum, col,
                                  "Expected: SYMBOL, Found: NUMBER");
                        }
                        // Symbols can be up to ten characters
                        if (accum.ToString().Length > 10)
                        {
                            throw new ScannerException(lineNum, col,
                                string.Format("Symbol '{0}' is too long",
                                              accum.ToString()));
                        }
                        yield return new Token { ColumnNumber = col,
                              Text = accum.ToString(), Type = TokenType.LABEL };
                        col = endCol;
                        break;

                    // Opcodes are special MIX keywords
                    case ScannerState.OPCODE:
                        // Here we check the text to see that it conforms
                        // to the MIX instruction set, or a pseudo op etc...
                        var tokText = accum.ToString().Trim();

                        if (MIXMachine.INSTRUCTION_LIST.FindIndex(
                                       i => i.Name == tokText) != -1)
                            yield return new Token { ColumnNumber = col,
                                  Text = tokText, Type = TokenType.KEYWORD };
                        else if (tokText == "ORIG")
                            yield return new Token { ColumnNumber = col,
                                  Text = tokText, Type = TokenType.ORIG };
                        else if (tokText == "EQU")
                            yield return new Token { ColumnNumber = col,
                                  Text = tokText, Type = TokenType.EQU };
                        else if (tokText == "CON")
                            yield return new Token { ColumnNumber = col,
                                  Text = tokText, Type = TokenType.CON };
                        else if (tokText == "ALF")
                        {
                            state = ScannerState.STRING;
                            yield return new Token { ColumnNumber = col,
                                  Text = tokText, Type = TokenType.ALF };
                        }
                        else if (tokText == "END")
                            yield return new Token { ColumnNumber = col,
                                  Text = tokText, Type = TokenType.END };
                        else
                            throw new ScannerException(lineNum, col,
                                string.Format("Expected: KEYWORD " +
                                       "or PSEUDO, Found: '{0}'", tokText));

                        col = endCol;
                        break;

                    // Operands can be any symbol or number
                    case ScannerState.OPERAND:
                        if (int.TryParse(accum.ToString(), out temp))
                        {
                            if (accum.ToString().Length > 10)
                            {
                                throw new ScannerException(lineNum, col,
                                    string.Format("Number '{0}' is too long",
                                                  accum.ToString()));
                            }
                            yield return new Token { ColumnNumber = col,
                                  Text = accum.ToString(), Type = TokenType.NUMBER };
                        }
                        else
                        {
                            if (accum.ToString().Length > 10)
                            {
                                throw new ScannerException(lineNum, col,
                                    string.Format("Symbol '{0}' is too long",
                                                  accum.ToString()));
                            }
                            yield return new Token { ColumnNumber = col,
                                  Text = accum.ToString(), Type = TokenType.SYMBOL };
                        }
                        col = endCol;
                        break;
                }
            }
            else if (ch == '/')
            {
                input.Read();
                endCol++;
                if ((char)input.Peek() == '/')
                {
                    input.Read();
                    endCol++;
                    yield return new Token { ColumnNumber = col, Text = "//",
                                             Type = TokenType.SLASHSLASH };
                }
                else
                    yield return new Token { ColumnNumber = col, Text = "/",
                                             Type = TokenType.SLASH };
                col = endCol;
            }
            else if (char.IsWhiteSpace(ch))
            {
                // Read the first blank
                input.Read();
                endCol++;

                if (state != ScannerState.STRING)
                {
                    // Change the scanner state
                    switch (state)
                    {
                        case ScannerState.LABEL:
                            state = ScannerState.OPCODE;
                            col = ReadToNextChar(input, col);
                            break;
                        case ScannerState.OPCODE:
                            state = ScannerState.OPERAND;
                            col = ReadToNextChar(input, col);
                            break;
                        case ScannerState.OPERAND:
                            yield break;
                    }
                }
                else
                {
                    ch = (char)input.Peek();
                    if (char.IsWhiteSpace(ch))
                    {
                        input.Read();
                        endCol++;
                        ch = (char)input.Peek();
                    }
                    string sc = ((char)input.Read()).ToString();

                    // Read four more characters
                    for (var i = 0; i < 4; i++)
                    {
                        if (input.Peek() == -1)
                            yield return new Token { ColumnNumber = col,
                                  Type = TokenType.STRING, Text = sc.PadRight(5) };
                        sc += (char)input.Read();
                    }
                    yield return new Token { ColumnNumber = col,
                          Type = TokenType.STRING, Text = sc };
                }
            }
            else
                switch (ch)
                {
                    case '+':
                        endCol++;
                        input.Read();
                        yield return new Token { ColumnNumber = col,
                              Text = "+", Type = TokenType.PLUS };
                        col = endCol;
                        break;
                    case '-':
                        endCol++;
                        input.Read();
                        yield return new Token { ColumnNumber = col,
                              Text = "-", Type = TokenType.MINUS };
                        col = endCol;
                        break;
                    case '*':
                        endCol++;
                        input.Read();
                        yield return new Token { ColumnNumber = col,
                              Text = "*", Type = TokenType.STAR };
                        col = endCol;
                        break;
                    case '(':
                        endCol++;
                        input.Read();
                        yield return new Token { ColumnNumber = col,
                              Text = "(", Type = TokenType.LPAREN };
                        col = endCol;
                        break;
                    case ')':
                        endCol++;
                        input.Read();
                        yield return new Token { ColumnNumber = col,
                              Text = ")", Type = TokenType.RPAREN };
                        col = endCol;
                        break;
                    case ',':
                        endCol++;
                        input.Read();
                        yield return new Token { ColumnNumber = col,
                              Text = ",", Type = TokenType.COMMA };
                        col = endCol;
                        break;
                    case ':':
                        endCol++;
                        input.Read();
                        yield return new Token { ColumnNumber = col,
                              Text = ":", Type = TokenType.COLON };
                        col = endCol;
                        break;
                    case '=':
                        endCol++;
                        input.Read();
                        yield return new Token { ColumnNumber = col,
                              Text = "=", Type = TokenType.EQUALS };
                        col = endCol;
                        break;
                    default:
                        throw new ScannerException(lineNum, col,
                          string.Format("Unexpected character: '{0}'", ch));
                }
        }
    }
}
```

The scanner may be in one of these states at any time: `LABEL`, `OPCODE`, `OPERAND`, or `STRING.` As soon as the scanner encounters whitespace, it changes state in this order: `LABEL` -> `OPCODE` -> `OPERAND`, unless the `OPCODE` is "ALF", in which case the sequence is `LABEL` -> `OPCODE` -> `STRING`. It may be surprising, but the scanner treats the "ALF" opcode specially, because it needs to know whether it will scan a string or an operand. This little detail should be the parser's job, but in this case, it makes things quite a bit easier. Also note the use of `yield return`. This allows us to query the scanner only when the next token is needed.

### Simulator Implementation

The MIX machine consists of various components:

- 4000 words of memory;
- various word-sized registers;
- around 150 instructions in its instruction set;
- various optional asynchronous I/O devices: tape drives, disks, a card punch, a line printer, paper tape, and a terminal.

These components have been encapsulated in C# classes, all glued together by the class `MIXMachine`.

The `MIXWord` class represents one word of MIX's memory: 5 bytes of 6 bits each. The most relevant portion of that class appears below:

```csharp
public class MIXWord
{
    #region Fields

    public int this[byte index]
    {
        get
        {
            if (index < 0 || index > WORD_SIZE)
                throw new Exception("Invalid word index.");

            if (index == 0)
                return Sign == Sign.Positive ? 0 : 1;

            string strBitmask = BYTE_ONE + Enumerable.Repeat(BYTE_ZERO,
                   (WORD_SIZE - index)).Aggregate("", (x, y) => x + y);
            int bitmask = Convert.ToInt32(strBitmask, 2);
            return (data & bitmask) >> ((WORD_SIZE - index) * BYTE_SIZE);
        }
        set
        {
            if (index < 0 || index > WORD_SIZE)
                throw new Exception("Invalid word index.");

            if (index == 0)
            {
                if (value == 1)
                    Sign = Sign.Negative;
                else if (value == 0)
                    Sign = Sign.Positive;
                else
                    throw new Exception("Invalid sign value.");
            }
            else
            {
                byte leftCount = (byte)(index - 1);
                byte rightCount = (byte)(WORD_SIZE - index);

                int bitmaskClear = Convert.ToInt32(
                    "1" + // Sign
                    Enumerable.Repeat(BYTE_ONE, leftCount).Aggregate("", (x, y) => x + y) +
                    BYTE_ZERO +
                    Enumerable.Repeat(BYTE_ONE, rightCount).Aggregate("", (x, y) => x + y),
                    2);

                string valString = Convert.ToString(Math.Abs(value), 2);
                valString = Enumerable.Repeat("0",
                  BYTE_SIZE - valString.Length).Aggregate("", (x, y) => x + y) + valString;

                int bitmaskSet = Convert.ToInt32(
                    "0" +
                    Enumerable.Repeat(BYTE_ZERO, leftCount).Aggregate("", (x, y) => x + y) +
                    valString +
                    Enumerable.Repeat(BYTE_ZERO, rightCount).Aggregate("", (x, y) => x + y),
                    2);

                data &= bitmaskClear;
                data |= bitmaskSet;
            }
        }
    }

    public int this[byte l, byte r]
    {
        get
        {
            // If the fieldspecs are given in the wrong order
            if (l > r)
            {
                byte t = r;
                r = l;
                l = t;
            }

            if (l < 0 || l > WORD_SIZE)
                throw new Exception("Invalid word index (left).");

            if (r < 0 || r > WORD_SIZE)
                throw new Exception("Invalid word index (right).");

            if (r == 0)
                return this[0];

            bool useSign = false;
            if (l == 0)
            {
                l++;
                useSign = true;
            }

            byte leftCount = (byte)(l - 1);
            byte midCount = (byte)(1 + (r - l));
            byte rightCount = (byte)(WORD_SIZE - r);

            int bitmask = Convert.ToInt32(
                Enumerable.Repeat(BYTE_ZERO, leftCount).Aggregate("", (x, y) => x + y) +
                Enumerable.Repeat(BYTE_ONE, midCount).Aggregate("", (x, y) => x + y) +
                Enumerable.Repeat(BYTE_ZERO, rightCount).Aggregate("", (x, y) => x + y),
                2);

            int result = (data & bitmask) >> (rightCount * BYTE_SIZE);

            if (useSign && Sign == Sign.Negative)
                return -result;

            return result;
        }
        set
        {
            // If the fieldspecs are given in the wrong order
            if (l > r)
            {
                byte t = r;
                r = l;
                l = t;
            }

            if (l < 0 || l > WORD_SIZE)
                throw new Exception("Invalid word index (left).");

            if (r < 0 || r > WORD_SIZE)
                throw new Exception("Invalid word index (right).");

            if (r == 0)
            {
                this[0] = value;
                return;
            }

            bool useSign = false;
            if (l == 0)
            {
                l++;
                useSign = true;
            }

            byte leftCount = (byte)(l - 1);
            byte midCount = (byte)(1 + (r - l));
            byte rightCount = (byte)(WORD_SIZE - r);

            int bitmaskClear = Convert.ToInt32(
                "1" + // Sign
                Enumerable.Repeat(BYTE_ONE, leftCount).Aggregate("", (x, y) => x + y) +
                Enumerable.Repeat(BYTE_ZERO, midCount).Aggregate("", (x, y) => x + y) +
                Enumerable.Repeat(BYTE_ONE, rightCount).Aggregate("", (x, y) => x + y),
                2);

            string valString = Convert.ToString(Math.Abs(value), 2);
            valString = Enumerable.Repeat("0",
              BYTE_SIZE * midCount - valString.Length).Aggregate("", (x, y) => x + y) +
              valString;

            int bitmaskSet = Convert.ToInt32(
                "0" + // Sign
                Enumerable.Repeat(BYTE_ZERO, leftCount).Aggregate("", (x, y) => x + y) +
                valString +
                Enumerable.Repeat(BYTE_ZERO, rightCount).Aggregate("", (x, y) => x + y),
                2);

            data &= bitmaskClear;
            data |= bitmaskSet;

            if (useSign)
            {
                // Clear sign
                data &= PLUS_MASK;
                Sign = value < 0 ? Sign.Negative : Sign.Positive;
            }
        }
    }

    #endregion
}
```

The `MIXWord` class has been implemented such that we may index it so we may obtain word slices. This has been done using bit masking.

MIX's asynchronous devices are all descendants of the `MIXDevice` abstract class:

```csharp
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
        get { return store; }
        set
        {
            lock (this)
            {
                Ready = false;
                store = value;
                StoreChanged();
                Ready = true;
            }
        }
    }

    public bool Ready { get; protected set; }

    private Stream store;

    protected virtual void StoreChanged() { }

    /// <summary>
    /// The device's block size in MIX words
    /// </summary>
    protected byte BlockSize { get; private set; }
    /// <summary>
    /// The MIX machine this device is attached to
    /// </summary>
    protected MIXMachine Machine { get; private set; }

    protected MIXDevice(MIXMachine machine, string name,
                        Stream store, byte blockSize)
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
            result += ch.DefaultIfEmpty('â–ˆ').First().ToString();
        }

        return result;
    }

    public override string ToString()
    {
        string result = "";
        if (Store == null)
            result = string.Format("NAME: {0}; BLOCK SIZE: {1}; BACKING STORE: N/A",
                                   Name, BlockSize);
        else if (Store is FileStream)
            result = string.Format("NAME: {0}, BLOCK SIZE: {1}, BACKING STORE: {2}",
                                   Name, BlockSize, (Store as FileStream).Name);
        else if (Store is MemoryStream)
            result = string.Format("NAME: {0}, BLOCK SIZE: {1}, BACKING STORE: MEMORY",
                                   Name, BlockSize);
        else
            result = string.Format("NAME: {0}, BLOCK SIZE: {1}, BACKING STORE: CONSOLE",
                                   Name, BlockSize);

        return string.Format(result);
    }

    public void Out(int M)
    {
        Thread t = new Thread(OutProc);
        t.Start(M);
    }

    public void In(int M)
    {
        Thread t = new Thread(InProc);
        t.Start(M);
    }

    public void IOC(int M)
    {
        Thread t = new Thread(IOCProc);
        t.Start(M);
    }

    protected abstract void OutProc(object M);
    protected abstract void InProc(object M);
    protected abstract void IOCProc(object M);
}
```

Concrete devices have been implemented by inheriting this class and overriding the `OutProc()`, `InProc()`, and `IOCProc()` methods. Each device has a backing store, which can either be a `MemoryStream`, a `FileStream`, or the console itself. The `Redirect()` method provides a means to change this backing store.

The final part of MIX's infrastructure is its instruction set. Each MIX instruction is a `MIXInstruction` object:

```csharp
public class MIXInstruction
{
    private Action<MIXWord, byte, byte> executionProc;
    public string Name { get; private set; }

    public MIXInstruction(string name, Action<MIXWord, byte, byte> executionProc)
    {
        this.Name = name;
        this.executionProc = executionProc;
    }

    public void Execute(MIXWord address, byte index, byte field)
    {
        executionProc(address, index, field);
    }

    public void Execute(MIXWord address, byte index, byte left, byte right)
    {
        executionProc(address, index, (byte)(left * 8 + right));
    }
}
```

To create a `MIXInstruction` object, we pass an anonymous method to its constructor, which is what gets executed once that instruction object is requested to execute:

```csharp
instructionTable = new List<MIXInstruction>();

#region Load Operators

instructionTable.Add(new MIXInstruction("LDA",
    (a, i, f) =>
    {
        int M = a + (i > 0 ? I[i - 1].Value : 0);
        A.Value = Memory[M][(byte)(f / 8), (byte)(f % 8)];
        PC++;
    }));
```

### Other Details

The MIX simulator interface, MIX, is command line driven. Chapter 3 of the documentation describes its outline. There is nothing in particular of note about this program. However, it is worth noting that the classes described above, which comprise the MIX computer, are stored in a separate library, so one is able to develop a GUI for the MIX machine instead of the command line interface provided with these tools. Such contributions are very welcome!