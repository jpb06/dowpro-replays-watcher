using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace DoWproReplayWatcher.Logic.Helpers
{
    public class ArchiveHelper
    {
        public static void CreateZipFile(string fileName, IEnumerable<string> files)
        {
            // Create and open a new ZIP file
            var zip = ZipFile.Open(fileName, ZipArchiveMode.Create, Encoding.GetEncoding(1252));
            foreach (var file in files)
            {
                // Add the entry for each file
                zip.CreateEntryFromFile(file, Path.GetFileName(file), CompressionLevel.Optimal);
            }
            // Dispose of the object when we are done
            zip.Dispose();
        }
    }
}
