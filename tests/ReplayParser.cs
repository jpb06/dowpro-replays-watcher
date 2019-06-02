using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace tests
{
    public static class ReplayParser
    {
        public static string GetMapName(string filePath)
        {
            using (FileStream stream = File.Open(filePath, FileMode.Open))
            {
                using (BinaryReader reader = new BinaryReader(stream, Encoding.ASCII))
                {
                    stream.Position = 234;
                    //[DWORD]->%length% // length of additional header infos
                    int length = reader.ReadInt32();
                    //[UNICODETEXT] // Double-byte engine version string (%length%*2)
                    byte[] buffer = reader.ReadBytes(length * 2);
                    //string engine = new UnicodeEncoding().GetString(buffer);
                    //[DWORD]->%length% // lenth of map Name
                    length = reader.ReadInt32();
                    //[TEXT] // Map name (%length%)
                    char[] chr = reader.ReadChars(length);

                    string fullName = new string(chr);

                    return fullName.Substring(fullName.LastIndexOf("\\")+1);
                }
            }
        }
    }
}
