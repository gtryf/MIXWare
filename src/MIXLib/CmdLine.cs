using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Linq;

namespace MIXLib.Util
{
    public static class CommandLineHelper
    {
        public static Dictionary<string, string> SplitCommandLine(string commandLine, Dictionary<string, string> aliases)
		    => SplitCommandLine(commandLine, aliases, RuntimeInformation.IsOSPlatform(OSPlatform.Windows));

        public static Dictionary<string, string> SplitCommandLine(string commandLine, Dictionary<string, string> aliases, bool noCase)
        {
            bool inQuotes = false;
            Dictionary<string, string> result = new Dictionary<string, string>();

            var argList = commandLine.Split(c =>
            {
                if (c == '\"')
                    inQuotes = !inQuotes;

                return !inQuotes && c == ' ';
            })
            .Select(arg =>
            {
                if (noCase)
                    return arg.Trim().TrimMatchingQuotes('\"').ToLower();
                else
                    return arg.Trim().TrimMatchingQuotes('\"');
            }).Where(arg => !string.IsNullOrEmpty(arg)).Skip(1);

            foreach (var a in argList)
            {
                var parts = a.Split(':');

                if (parts.Length == 1)
                {
                    if (aliases.ContainsKey(parts.First()))
                        result.Add(aliases[parts.First()], null);
                    else
                        result.Add(parts.First(), null);
                }
                else
                {
                    string k = parts.First();
                    string v = string.Join(":", parts.Skip(1).ToArray());

                    if (aliases.ContainsKey(k))
                        k = aliases[k];

                    result.Add(k, v);
                }
            }
            
            return result;
        }

        public static IEnumerable<string> Split(this string str,
                                            Func<char, bool> controller)
        {
            int nextPiece = 0;

            for (int c = 0; c < str.Length; c++)
            {
                if (controller(str[c]))
                {
                    yield return str.Substring(nextPiece, c - nextPiece);
                    nextPiece = c + 1;
                }
            }

            yield return str.Substring(nextPiece);
        }

        public static string TrimMatchingQuotes(this string input, char quote)
        {
            if ((input.Length >= 2) &&
                (input[0] == quote) && (input[input.Length - 1] == quote))
                return input.Substring(1, input.Length - 2);

            return input;
        }
    }
}