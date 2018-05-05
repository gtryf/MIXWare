using System;
using System.IO;
using System.Text;
using System.Linq;
using System.Collections.Generic;

namespace MIXLib.Parser
{
    public enum TokenType
    {
        LABEL,
        SYMBOL,
        NUMBER,
        KEYWORD,
        STRING,
        EQU,
        ORIG,
        CON,
        ALF,
        END,
        PLUS,
        MINUS,
        STAR,
        SLASH,
        SLASHSLASH,
        LPAREN,
        RPAREN,
        COMMA,
        COLON,
        EQUALS
    }

    public struct Token
    {
        public TokenType Type;
        public string Text;
        public byte ColumnNumber;
    }

    public class ScannerException
        : Exception
    {
        public int LineNumber { get; private set; }
        public byte ColumnNumber { get; private set; }

        public ScannerException(int l, byte c, string m)
            : base(m)
        {
            LineNumber = l;
            ColumnNumber = c;
        }
    }

    public sealed class Scanner
    {
        private enum ScannerState
        {
            LABEL,
            OPCODE,
            OPERAND,
            STRING
        }

        private readonly string line;
        private readonly int lineNum;

        public Scanner(int lineNum, string input)
        {
            this.line = input;
            this.lineNum = lineNum;
        }

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
                                    throw new ScannerException(lineNum, col, "Expected: SYMBOL, Found: NUMBER");
                                }
                                // Symbols can be up to ten characters
                                if (accum.ToString().Length > 10)
                                {
                                    throw new ScannerException(lineNum, col, 
                                        string.Format("Symbol '{0}' is too long", accum.ToString()));
                                }
                                yield return new Token { ColumnNumber = col, Text = accum.ToString(), Type = TokenType.LABEL };
                                col = endCol;
                                break;

                            // Opcodes are special MIX keywords
                            case ScannerState.OPCODE:
                                // Here we check the text to see that it conforms to the MIX instruction set, or a pseudo op etc...
                                var tokText = accum.ToString().Trim();

                                if (MIXMachine.INSTRUCTION_LIST.FindIndex(i => i.Name == tokText) != -1)
                                    yield return new Token { ColumnNumber = col, Text = tokText, Type = TokenType.KEYWORD };
                                else if (tokText == "ORIG")
                                    yield return new Token { ColumnNumber = col, Text = tokText, Type = TokenType.ORIG };
                                else if (tokText == "EQU")
                                    yield return new Token { ColumnNumber = col, Text = tokText, Type = TokenType.EQU };
                                else if (tokText == "CON")
                                    yield return new Token { ColumnNumber = col, Text = tokText, Type = TokenType.CON };
                                else if (tokText == "ALF")
                                {
                                    state = ScannerState.STRING;
                                    yield return new Token { ColumnNumber = col, Text = tokText, Type = TokenType.ALF };
                                }
                                else if (tokText == "END")
                                    yield return new Token { ColumnNumber = col, Text = tokText, Type = TokenType.END };
                                else
                                    throw new ScannerException(lineNum, col, 
                                        string.Format("Expected: KEYWORD or PSEUDO, Found: '{0}'", tokText));

                                col = endCol;
                                break;

                            // Operands can be any symbol or number
                            case ScannerState.OPERAND:
                                if (int.TryParse(accum.ToString(), out temp))
                                {
                                    if (accum.ToString().Length > 10)
                                    {
                                        throw new ScannerException(lineNum, col,
                                            string.Format("Number '{0}' is too long", accum.ToString()));
                                    }
                                    yield return new Token { ColumnNumber = col, Text = accum.ToString(), Type = TokenType.NUMBER };
                                }
                                else
                                {
                                    if (accum.ToString().Length > 10)
                                    {
                                        throw new ScannerException(lineNum, col,
                                            string.Format("Symbol '{0}' is too long", accum.ToString()));
                                    }
                                    yield return new Token { ColumnNumber = col, Text = accum.ToString(), Type = TokenType.SYMBOL };
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
                            yield return new Token { ColumnNumber = col, Text = "//", Type = TokenType.SLASHSLASH };
                        }
                        else
                            yield return new Token { ColumnNumber = col, Text = "/", Type = TokenType.SLASH };
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
                                    yield return new Token { ColumnNumber = col, Type = TokenType.STRING, Text = sc.PadRight(5) };
                                sc += (char)input.Read();
                            }
                            yield return new Token { ColumnNumber = col, Type = TokenType.STRING, Text = sc };
                        }
                    }
                    else
                        switch (ch)
                        {
                            case '+':
                                endCol++;
                                input.Read();
                                yield return new Token { ColumnNumber = col, Text = "+", Type = TokenType.PLUS };
                                col = endCol;
                                break;
                            case '-':
                                endCol++;
                                input.Read();
                                yield return new Token { ColumnNumber = col, Text = "-", Type = TokenType.MINUS };
                                col = endCol;
                                break;
                            case '*':
                                endCol++;
                                input.Read();
                                yield return new Token { ColumnNumber = col, Text = "*", Type = TokenType.STAR };
                                col = endCol;
                                break;
                            case '(':
                                endCol++;
                                input.Read();
                                yield return new Token { ColumnNumber = col, Text = "(", Type = TokenType.LPAREN };
                                col = endCol;
                                break;
                            case ')':
                                endCol++;
                                input.Read();
                                yield return new Token { ColumnNumber = col, Text = ")", Type = TokenType.RPAREN };
                                col = endCol;
                                break;
                            case ',':
                                endCol++;
                                input.Read();
                                yield return new Token { ColumnNumber = col, Text = ",", Type = TokenType.COMMA };
                                col = endCol;
                                break;
                            case ':':
                                endCol++;
                                input.Read();
                                yield return new Token { ColumnNumber = col, Text = ":", Type = TokenType.COLON };
                                col = endCol;
                                break;
                            case '=':
                                endCol++;
                                input.Read();
                                yield return new Token { ColumnNumber = col, Text = "=", Type = TokenType.EQUALS };
                                col = endCol;
                                break;
                            default:
                                throw new ScannerException(lineNum, col, string.Format("Unexpected character: '{0}'", ch));
                        }
                }
            }
        }

        private byte ReadToNextChar(TextReader input, byte currCol)
        {
            char ch = (char)input.Peek();

            while (char.IsWhiteSpace(ch))
            {
                input.Read();
                currCol++;

                ch = (char)input.Peek();
            }

            return currCol;
        }
    }
}