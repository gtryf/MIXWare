using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using MIXLib.Parser;
using MIXUI.Helpers;

namespace MIXUI.Assembler
{
    public class BinaryAssembler : AbstractAssembler
    {
        public override Union<AssemblySuccessResult, AssemblyErrorResult> Assemble(string sourceFileName, string text, bool produceSymbolTable, bool produceListing)
        {
            var parser = new Parser(new StringReader(text));
            parser.ParseProgram();

            if (!parser.Errors.Any())
            {
                var assembly = new BinaryAssembly
                {
                    StartLoc = parser.StartLoc,
                    Assembly = parser.Assembly,
                    SymbolTable = parser.SymbolTable
                };
                var serializer = new XmlSerializer(typeof(BinaryAssembly));
                var xml = new StringBuilder();
                var writer = new StringWriter(xml);
                serializer.Serialize(writer, assembly);
                var raw = Encoding.UTF8.GetBytes(writer.ToString());

                AssemblySuccessResult result;
                using (MemoryStream memory = new MemoryStream())
                {
                    using (GZipStream gzip = new GZipStream(memory, CompressionMode.Compress, true))
                    {
                        gzip.Write(raw, 0, raw.Length);
                    }
                    result = new AssemblySuccessResult(parser.Assembly.Count(), memory.ToArray());
                }

                if (produceListing)
                    result.Listing = MakeListing(parser, text);
                if (produceSymbolTable)
                    result.SymbolTable = MakeSymbolTable(parser);

                return new Union<AssemblySuccessResult, AssemblyErrorResult>.Case1(result);
            }
            else
                return new Union<AssemblySuccessResult, AssemblyErrorResult>.Case2(new AssemblyErrorResult(parser.Errors, parser.Warnings));
        }
    }
}
