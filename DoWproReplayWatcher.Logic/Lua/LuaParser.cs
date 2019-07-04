using DoWproReplayWatcher.Logic.Extensions;
using DoWproReplayWatcher.Logic.Lua;
using DoWproReplayWatcher.Logic.Types;
using System;
using System.Globalization;
using System.IO;
namespace DoWproReplayWatcher.Lua
{
    public class LuaParser
    {
        public static GameResult ParseGameResult(string filePath)
        {
            if (!filePath.EndsWith(".lua", true, CultureInfo.InvariantCulture))
                throw new Exception("Not a lua file (invalid extension)");

            if (!File.Exists(filePath))
                throw new Exception("File not found");

            string fileContent = File.ReadAllText(filePath).ClearLua();

            LuaObject root = Parse(fileContent);

            return root.AsGameResult();
        }

        private static LuaObject Parse(string rawContent)
        {
            LuaObject root = new LuaObject();

            while (rawContent.Length > 0)
            {
                if (rawContent.StartsWith(","))
                    rawContent = rawContent.Substring(1);

                int equalIndex = rawContent.IndexOf("=");
                int commaIndex = rawContent.IndexOf(",");
                int bracketIndex = rawContent.IndexOf("{");

                if (equalIndex == -1 && commaIndex == -1 && bracketIndex == -1)
                    break;

                string identifier = rawContent.Substring(0, equalIndex);
                string value = string.Empty;
                
                if (bracketIndex == -1 || commaIndex < bracketIndex)
                {
                    value = rawContent.Substring(equalIndex + 1, commaIndex - equalIndex - 1);
                    rawContent = rawContent.Substring(commaIndex + 1);
                }
                else
                {
                    int endBracketIndex = FindMatchingBracketIndex(rawContent, bracketIndex == -1 ? 0 : bracketIndex);
                    value = rawContent.Substring(equalIndex + 1, endBracketIndex);
                    rawContent = rawContent.Substring(equalIndex +endBracketIndex + 1);
                }

                if (value.StartsWith("{") && value.EndsWith("}"))
                {
                    LuaObject sub = Parse(value.Substring(1, value.Length - 2));
                    root.Add(identifier, sub);
                }
                else
                {
                    if (Int32.TryParse(value, out int intValue))
                        root.Add(identifier, intValue);
                    else
                        root.Add(identifier, value.Substring(1, value.Length - 2));
                }
            }

            return root;
        }

        private static int FindMatchingBracketIndex(
            string value,
            int startIndex)
        {
            int skip = 0;
            string subSet = value.Substring(startIndex);

            for (int i = 1; i < subSet.Length; i++)
            {
                if (skip == 0 && subSet[i] == '}')
                    return i+1;

                if (subSet[i] == '{') skip++;
                if (subSet[i] == '}') skip--;
            }

            return subSet.Length;
        }
    }


}
