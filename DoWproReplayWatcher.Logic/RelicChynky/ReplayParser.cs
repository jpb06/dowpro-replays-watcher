using DoWproReplayWatcher.Logic.Types;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DoWproReplayWatcher.RelicChunky
{
    public static class ReplayParser
    {
        public static RelicChunkyData Fetch(string filePath)
        {
            string mapName = string.Empty;
            string modName = string.Empty;
            string replayName = string.Empty;
            long replayNameLengthPosition = 0;
            long replayNameValueEndPosition = 0;
            long databaseChunkLengthPosition = 0;
            long headerLengthPosition = 0;
            int databaseChunkLength = 0;
            int headerLength = 0;

            using (FileStream stream = File.Open(filePath, FileMode.Open))
            {
                using (BinaryReader reader = new BinaryReader(stream, Encoding.ASCII))
                {
                    //[DWORD]->%length% // length of mod name
                    int length = reader.ReadInt32();
                    //[UNICODETEXT] // Double-byte engine version string (%length%*2)
                    char[] chr = reader.ReadChars(length);
                    modName = new string(chr).TrimEnd(new char[] { '\0' });

                    headerLengthPosition = stream.Position = 153;
                    //[DWORD]->%length% // length of header
                    headerLength = reader.ReadInt32();

                    stream.Position = 234;
                    //[DWORD]->%length% // length of map name
                    length = reader.ReadInt32();
                    //[UNICODETEXT] // Double-byte map name string (%length%*2)
                    byte[] buffer = reader.ReadBytes(length * 2);

                    //[DWORD]->%length% // length of map internal name
                    length = reader.ReadInt32();
                    //[TEXT] // Map internal name (%length%)
                    chr = reader.ReadChars(length);

                    mapName = new string(chr);
                    mapName = mapName.Substring(mapName.LastIndexOf("\\") + 1);

                    reader.BaseStream.Position += 16;

                    reader.BaseStream.Position += 8; // DATABASE
                    reader.BaseStream.Position += 4; // ?

                    databaseChunkLengthPosition = reader.BaseStream.Position;
                    //[DWORD]->%length% // length of DATABASE chunk
                    databaseChunkLength = reader.ReadInt32();
                    reader.BaseStream.Position += 24;

                    reader.BaseStream.Position += 61; // game options

                    replayNameLengthPosition = reader.BaseStream.Position;

                    //[DWORD]->%length% // length of replay name
                    length = reader.ReadInt32();
                    //[UNICODETEXT] // Double-byte replay name string (%length%*2)
                    buffer = reader.ReadBytes(length * 2);
                    replayName = new UnicodeEncoding().GetString(buffer);

                    replayNameValueEndPosition = reader.BaseStream.Position;
                }

                if (string.IsNullOrEmpty(modName) || 
                    string.IsNullOrEmpty(mapName) ||
                    string.IsNullOrEmpty(replayName)) return null;

                return new RelicChunkyData
                {
                    ModName = modName,
                    MapName = mapName,
                    ReplayName = replayName,

                    ReplayNameLengthPosition = replayNameLengthPosition,
                    ReplayNameValueEndPosition = replayNameValueEndPosition,
                    DatabaseChunkLengthPosition = databaseChunkLengthPosition,
                    HeaderLengthPosition = headerLengthPosition,

                    DatabaseChunkLength = databaseChunkLength,
                    HeaderLength = headerLength
                };

            }
        }
    }
}
