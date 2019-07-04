using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DoWproReplayWatcher.Logic.Extensions
{
    public static class StringExtensions
    {
        public static string RemoveWhitespace(this string input)
        {
            return new string(input
                .ToCharArray()
                .Where(c => !Char.IsWhiteSpace(c))
                .ToArray());
        }

        public static string ClearLua(this string input)
        {
            string cleaned = Regex.Replace(input, @"(\t(.+))( = )(.+),\r\n", "$2=$4,");
            cleaned = Regex.Replace(cleaned, @" =  \r\n", "=");
            cleaned = Regex.Replace(cleaned, @"\t", "");
            cleaned = Regex.Replace(cleaned, @"\r\n", "");

            return cleaned;
        }
    }
}
