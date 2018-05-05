using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace MIXLib.Parser
{
    /* The MIXAL Grammar:
     * 
     * A program is a sequence of statements followed by an END statement:
     * 
     * <prog>        --> <statement>+ PSEUDO["END"] <w_value>
     * 
     * A statement is either a label or an instruction
     * 
     * <statement>   --> LABEL | <instruction>
     * 
     * An instruction is a keyword or a pseudo operation followed by an operand
     * 
     * <instruction> --> PSEUDO <w_value> |
     *                   KEYWORD <a_part> <i_part> <f_part>
     *                   
     * <a_part>  --> epsilon | <expression> | <literal>
     * <i_part>  --> epsilon | COMA <expression>
     * <f_part>  --> epsilon | LPAREN <expression> RPAREN
     * <w_value_part> --> <expression> <f_part>
     * <w_value> --> <w_value_part> | <w_value> COMA <w_value_part>
     * <literal> --> EQUALS <w_value> EQUALS
     * 
     * <atom> --> SYMBOL | NUMBER | STAR
     * <expression> --> <atom> |
     *                  PLUS <atom> |
     *                  MINUS <atom> |
     *                  <expression> PLUS <atom> |
     *                  <expression> MINUS <atom> |
     *                  <expression> STAR <atom> |
     *                  <expression> SLASH <atom> |
     *                  <expression> SLASHSLASH <atom> |
     *                  <expression> COLON <atom>
     */

    public struct ErrorInfo
    {
        public int Line;
        public byte Column;
        public string Text;
    }

    public class Parser
    {
        private struct FutureReference
        {
            public int Location;
            public int SourceLocation;
            public string Symbol;
            public MIXWord Index;
            public MIXWord Field;
            public MIXWord OpCode;
        }

        private Dictionary<byte, int> localSymbs;

        private int locCounter;
        private int insertionPoint;
        private Dictionary<string, MIXWord> symbolTable;
        private IEnumerable<Token> tokStream;

        private IList<ErrorInfo> errors;
        private IList<ErrorInfo> warnings;
        private List<MemoryCell> assembly;
        private List<FutureReference> futureRefs;

        private readonly TextReader reader;

        public IEnumerable<MemoryCell> Assembly
        {
            get { return assembly.AsEnumerable(); }
        }

        public Dictionary<string, MIXWord> SymbolTable
        {
            get { return symbolTable; }
        }

        public IEnumerable<ErrorInfo> Errors
        {
            get { return errors.AsEnumerable(); }
        }

        public IEnumerable<ErrorInfo> Warnings
        {
            get { return warnings.AsEnumerable(); }
        }

        public int LineNumber { get; set; }

        public MIXWord StartLoc { get; private set; }

        public Parser(TextReader reader)
        {
            locCounter = insertionPoint = 0;
            symbolTable = new Dictionary<string, MIXWord>();

            assembly = new List<MemoryCell>();
            errors = new List<ErrorInfo>();
            warnings = new List<ErrorInfo>();
            futureRefs = new List<FutureReference>();
            localSymbs = new Dictionary<byte, int>();
            LineNumber = 1;
            StartLoc = null;
            this.reader = reader;
        }

        private MIXWord Atom()
        {
            var tok = tokStream.First();
            tokStream = tokStream.Skip(1);

            if (tok.Type == TokenType.SYMBOL)
            {
                var symbName = tok.Text;
                if (IsLocal(symbName))
                {
                    var n = byte.Parse(symbName[0].ToString());
                    if (localSymbs.ContainsKey(n))
                        symbName = "|" + symbName[0] + "-" + localSymbs[n] + "|";
                }
                if (!symbolTable.ContainsKey(symbName))
                    errors.Add(new ErrorInfo { Text = "ATOM: Undefined symbol: " + symbName, Line = LineNumber, Column = tok.ColumnNumber });
                else
                    return symbolTable[symbName];
            }
            else if (tok.Type == TokenType.NUMBER)
                return new MIXWord(int.Parse(tok.Text));
            else if (tok.Type == TokenType.STAR)
                return new MIXWord(locCounter);
            else
                errors.Add(new ErrorInfo { Text = "ATOM: Unexpected token: " + tok.Type, Line = LineNumber, Column = tok.ColumnNumber });

            return 0;
        }

        private MIXWord Expression()
        {
            MIXWord result = new MIXWord();
            Set<TokenType> firstTerm = new Set<TokenType>();

            firstTerm.Add(TokenType.PLUS); firstTerm.Add(TokenType.MINUS);
            firstTerm.Add(TokenType.STAR); firstTerm.Add(TokenType.SLASH);
            firstTerm.Add(TokenType.SLASHSLASH); firstTerm.Add(TokenType.COLON);

            if (tokStream.First().Type == TokenType.PLUS)
            {
                tokStream = tokStream.Skip(1);
                result = Atom();
            }
            else if (tokStream.First().Type == TokenType.MINUS)
            {
                tokStream = tokStream.Skip(1);
                result = -Atom();
            }
            else
                result = Atom();

            while (tokStream.ToArray().Length > 0 && firstTerm.Contains(tokStream.First().Type))
            {
                var oper = tokStream.First().Type;
                tokStream = tokStream.Skip(1);

                var term = Atom();

                switch (oper)
                {
                    case TokenType.PLUS:
                        result += term;
                        break;
                    case TokenType.MINUS:
                        result -= term;
                        break;
                    case TokenType.STAR:
                        result *= term;
                        break;
                    case TokenType.SLASH:
                        result /= term;
                        break;
                    case TokenType.SLASHSLASH:
                        string wordZero = "";
                        for (int i = 0; i < 30; i++)
                            wordZero += "0";
                        long n = Convert.ToInt64(Convert.ToString(result.Value, 2) + wordZero, 2);
                        n /= (long)term.Value;
                        result = new MIXWord((int)n);
                        break;
                    case TokenType.COLON:
                        result = new MIXWord(8 * result + term);
                        break;
                }
            }

            return result;
        }

        public MIXWord Index()
        {
            MIXWord result = new MIXWord();

            if (tokStream.ToArray().Length > 0 && tokStream.First().Type == TokenType.COMMA)
            {
                tokStream = tokStream.Skip(1);
                result = Expression();
            }

            return result;
        }

        public MIXWord Field()
        {
            MIXWord result = new MIXWord(-1);

            if (tokStream.ToArray().Length > 0 && tokStream.First().Type == TokenType.LPAREN)
            {
                tokStream = tokStream.Skip(1);
                result = Expression();
                if (tokStream.First().Type != TokenType.RPAREN)
                {
                    errors.Add(new ErrorInfo
                    {
                        Column = tokStream.First().ColumnNumber,
                        Line = LineNumber,
                        Text = string.Format("FIELD: Unexpected input: '{0}', Expected: ')'", tokStream.First().Text)
                    });
                }
                tokStream = tokStream.Skip(1);
            }

            return result;
        }

        private bool IsLocal(string s)
        {
            return s.Length == 2 && char.IsDigit(s[0]) && s[1] == 'B';
        }

        private string FRefSymb;
        public MIXWord Address()
        {
            MIXWord result = new MIXWord();

            if (tokStream.ToArray().Length > 0)
            {
                var first = tokStream.First();

                if (first.Type == TokenType.SYMBOL)
                {
                    var symbName = first.Text;
                    if (symbName.Length == 2 && char.IsDigit(symbName[0]) && symbName[1] == 'F')
                    {
                        var n = byte.Parse(symbName[0].ToString());
                        if (!localSymbs.ContainsKey(n))
                            symbName = "|" + n + "-1|";
                        else
                            symbName = "|" + n + "-" + (localSymbs[n] + 1) + "|";
                    }
                    
                    if (!IsLocal(symbName) && !symbolTable.ContainsKey(symbName))
                    {
                        FRefSymb = symbName;
                        tokStream = tokStream.Skip(1);
                        return null;
                    }
                    else
                        result = Expression();
                }
                else if (first.Type == TokenType.EQUALS)
                {
                    tokStream = tokStream.Skip(1);
                    MIXWord litVal = WordValue();
                    if (first.Type != TokenType.EQUALS)
                        errors.Add(new ErrorInfo
                        {
                            Column = first.ColumnNumber,
                            Line = LineNumber,
                            Text = "ADDRESS: Expected: '='"
                        });
                    tokStream = tokStream.Skip(1);

                    FRefSymb = "=" + litVal.Value + "=";
                    return null;
                }
                else
                    result = Expression();
            }

            return result;
        }

        public MIXWord WordValue()
        {
            MIXWord result = 0;
            MIXWord e = Expression();
            int f = (int)Field(); if (f == -1) f = 5;
            byte w = (byte)(f % 8 - f / 8);

            result[(byte)(f / 8), (byte)(f % 8)] = e[(byte)(5 - w), 5];

            while (tokStream.ToArray().Length > 0 && tokStream.First().Type == TokenType.COMMA)
            {
                tokStream = tokStream.Skip(1);
                e = Expression();
                f = (int)Field(); if (f == -1) f = 5;
                w = (byte)(f % 8 - f / 8);

                result[(byte)(f / 8), (byte)(f % 8)] = e[(byte)(5 - w), 5];
            }

            return result;
        }

        private void ParseLine(Scanner s)
        {
            tokStream = s.Tokens;
            var lblText = string.Empty;

            if (tokStream.First().Type == TokenType.LABEL)
            {
                lblText = tokStream.First().Text;
                if (lblText.Length == 2 && char.IsDigit(lblText[0]) && lblText[1] == 'H')
                // This is a local symbol
                {
                    var n = byte.Parse(lblText[0].ToString());
                    if (localSymbs.ContainsKey(n))
                        localSymbs[n] = localSymbs[n] + 1;
                    else
                        localSymbs.Add(n, 1);
                    lblText = "|" + n + "-" + localSymbs[n] + "|";
                }
                if (tokStream.Skip(1).ToArray().Length > 0)
                {
                    symbolTable.Add(lblText, new MIXWord(locCounter));
                    tokStream = tokStream.Skip(1);
                }
                else
                    errors.Add(new ErrorInfo
                    {
                        Column = tokStream.First().ColumnNumber,
                        Line = LineNumber,
                        Text = "LINE: Unexpected end of line, expected: KEYWORD or PSEUDO"
                    });
            }

            // Parse the rest of the sentence
            switch (tokStream.First().Type)
            {
                case TokenType.KEYWORD:
                    var instrList = from i in MIXMachine.INSTRUCTION_LIST
                                    where i.Name == tokStream.First().Text
                                    select i;
                    InstructionInfo instr = instrList.First();
                    tokStream = tokStream.Skip(1);
                    MIXWord a = Address(); MIXWord index = Index(); MIXWord f = Field(); if (f == -1) f = instr.DefaultField;
                    MIXWord word = new MIXWord();
                    if (a != null)
                    {
                        word[0, 2] = a; word[3] = index; word[4] = f; word[5] = instr.OpCode;
                        assembly.Add(new MemoryCell { SourceLocation = LineNumber, Location = locCounter, Contents = word });
                    }
                    else
                    {
                        futureRefs.Add(new FutureReference { Symbol = FRefSymb, Field = f, Index = index, Location = locCounter, OpCode = instr.OpCode, SourceLocation = LineNumber });
                    }
                    
                    locCounter++;
                    break;
                case TokenType.ORIG:
                    tokStream = tokStream.Skip(1);
                    locCounter = WordValue();
                    break;
                case TokenType.CON:
                    tokStream = tokStream.Skip(1);
                    assembly.Add(new MemoryCell { SourceLocation = LineNumber, Location = locCounter, Contents = WordValue() });
                    locCounter++;
                    break;
                case TokenType.EQU:
                    tokStream = tokStream.Skip(1);
                    var val = WordValue();
                    if (!string.IsNullOrEmpty(lblText))
                        symbolTable[lblText] = val;
                    break;
                case TokenType.ALF:
                    tokStream = tokStream.Skip(1);
                    if (tokStream.First().Type != TokenType.STRING)
                        errors.Add(new ErrorInfo
                        {
                            Line = LineNumber,
                            Column = tokStream.First().ColumnNumber,
                            Text = "LINE: Expected: STRING CONSTANT."
                        });
                    else
                    {
                        string sc = tokStream.First().Text;

                        MIXWord w = new MIXWord();
                        for (byte i = 0; i < 5; i++)
                            w[(byte)(i + 1)] = MIXMachine.CHAR_TABLE[sc[i]];

                        assembly.Add(new MemoryCell { SourceLocation = LineNumber, Location = locCounter, Contents = w });
                        locCounter++;

                        tokStream = tokStream.Skip(1);
                    }
                    break;
                case TokenType.END:
                    if (null != StartLoc)
                        warnings.Add(new ErrorInfo
                        {
                            Line = LineNumber,
                            Column = tokStream.First().ColumnNumber,
                            Text = "LINE: Multiple appearances of the END directive."
                        });
                    tokStream = tokStream.Skip(1);
                    StartLoc = WordValue();
                    insertionPoint = locCounter;
                    break;
                default:
                    errors.Add(new ErrorInfo
                    {
                        Line = LineNumber,
                        Column = tokStream.First().ColumnNumber,
                        Text = string.Format("LINE: Parser panic! Don't know what to do with token {0}['{1}']",
                            tokStream.First().Type, tokStream.First().Text)
                    });
                    break;
            }
        }

        public void ParseProgram()
        {
            while (reader.Peek() != -1)
            {
                string line = reader.ReadLine();
                if (string.IsNullOrEmpty(line.Trim()) || line[0] == '*')
                {
                    LineNumber++;
                    continue;
                }

                Scanner s = new Scanner(LineNumber, line);
                try
                {
                    ParseLine(s);
                }
                catch (ScannerException e)
                {
                    errors.Add(new ErrorInfo
                    {
                        Column = e.ColumnNumber,
                        Line = e.LineNumber,
                        Text = "PROGRAM/SCANNER: " + e.Message
                    });
                }
                catch (InvalidOperationException)
                {
                    errors.Add(new ErrorInfo
                    {
                        Column = (byte)(line.Length - 1),
                        Line = LineNumber,
                        Text = "PROGRAM: Line ended unexpectedly"
                    });
                }
                finally
                {
                    LineNumber++;
                }
            }

            if (StartLoc == null)
                errors.Add(new ErrorInfo
                {
                    Column = 0,
                    Line = LineNumber,
                    Text = "PROGRAM: End of file reached and no END directive was found."
                });

            // Fix the future references
            MakeFutureRefs();
        }

        private void MakeFutureRefs()
        {
            foreach (var fr in futureRefs)
            {
                if (!symbolTable.ContainsKey(fr.Symbol))
                {
                    if (fr.Symbol.StartsWith("="))
                    {
                        int litVal = int.Parse(fr.Symbol.Substring(1, fr.Symbol.Length - 2));
                        assembly.Add(new MemoryCell { Location = insertionPoint, SourceLocation = 0, Contents = new MIXWord(litVal) });
                    }
                    else
                        assembly.Add(new MemoryCell { Location = insertionPoint, SourceLocation = 0, Contents = new MIXWord() });

                    symbolTable.Add(fr.Symbol, new MIXWord(insertionPoint));
                    insertionPoint++;
                }

                MIXWord word = new MIXWord();
                word[0, 2] = symbolTable[fr.Symbol];
                word[3] = fr.Index;
                word[4] = fr.Field;
                word[5] = fr.OpCode;

                assembly.Add(new MemoryCell { Location = fr.Location, SourceLocation = fr.SourceLocation, Contents = word });
            }
        }
    }
}