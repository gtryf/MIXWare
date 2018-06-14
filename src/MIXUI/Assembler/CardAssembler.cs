using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using MIXLib;
using MIXLib.Parser;
using MIXUI.Helpers;

namespace MIXUI.Assembler
{
    public class CardAssembler : AbstractAssembler
    {
        public override Union<AssemblySuccessResult, AssemblyErrorResult> Assemble(string sourceFileName, string text, bool produceSymbolTable, bool produceListing)
        {
            var parser = new Parser(new StringReader(text));
            parser.ParseProgram();

            if (!parser.Errors.Any())
            {
                var deck = new StringBuilder();
                using (var writer = new StringWriter(deck))
                {
                    // Write out the loading routine
                    writer.WriteLine(" O O6 Z O6    I C O4 0 EH A  F F CF 0  E   EU 0 IH G BB   EJ  CA. Z EU   EH E BA");
                    writer.WriteLine("   EU 2A-H S BB  C U 1AEH 2AEN V  E  CLU  ABG Z EH E BB J B. A  9               ");

                    byte remainder = (byte)(parser.Assembly.Count() % 7);
                    string comment = Path.GetFileNameWithoutExtension(sourceFileName).PadRight(5).Substring(0, 5).ToUpper();

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

                    var result = new AssemblySuccessResult(parser.Assembly.Count(), Encoding.UTF8.GetBytes(deck.ToString()));
                    if (produceListing)
                        result.Listing = MakeListing(parser, text);
                    if (produceSymbolTable)
                        result.SymbolTable = MakeSymbolTable(parser);

                    return new Union<AssemblySuccessResult, AssemblyErrorResult>.Case1(result);
                }
            }
            else
                return new Union<AssemblySuccessResult, AssemblyErrorResult>.Case2(new AssemblyErrorResult(parser.Errors, parser.Warnings));
        }
    }
}
