using DoWproReplayWatcher.Logic.Extensions;
using DoWproReplayWatcher.Logic.Types;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DoWproReplayWatcher.Logic.Helpers
{
    public class FileHelper
    {
        public static bool IsOpened(string path)
        {
            FileStream stream = null;
            bool isFileOpened = false;

            try
            {
                stream = new FileStream(path, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
                isFileOpened = false;
            }
            catch(Exception ex) when (ex is UnauthorizedAccessException || ex is IOException)
            {
                isFileOpened = true;
            }
            catch(Exception)
            {
                throw;
            }
            finally
            {
                if(stream != null) stream.Close();
            }

            return isFileOpened;
        }

        public static bool IsAlreadySaved(
            string directoryPath,
            string fileToCheckName)
        {
            if (Directory.Exists(directoryPath))
            {
                DirectoryInfo directory = new DirectoryInfo(directoryPath);

                var files = directory
                    .GetFiles("*.rec");

                var tempFile = files.Where(el => el.Name == "temp.rec").FirstOrDefault();
                if (tempFile == null)
                    return true; // we want process to sleep if temp.rec does not exist yet

                var fileToCheck = files.Where(el => el.Name == fileToCheckName).FirstOrDefault();
                if (fileToCheck == null)
                    return true; // we want process to sleep if target file does not exist yet

                var matchingSize = files
                    .Where(el => el.Name != "temp.rec")
                    .Where(el => el.Name != fileToCheckName)
                    .Where(el => el.Length == fileToCheck.Length);

                if (!matchingSize.Any()) return false;

                string fileToCheckHash = CryptoHelper.GetChecksum(Path.Combine(directoryPath, fileToCheckName));

                foreach(var match in matchingSize)
                {
                    string hash = CryptoHelper.GetChecksum(match.FullName);

                    if (hash == fileToCheckHash) return true;
                }

                return false;
            }
            else
            {
                throw new Exception("Directory doesn't exist");
            }
        }

        public static string GetPlayerProfile()
        {
            string playerProfileLine = File.ReadAllLines(Path.Combine(Constants.SoulstormInstallPath, "Local.ini"))
                .Where(el => el.StartsWith("playerprofile="))
                .FirstOrDefault();

            if (playerProfileLine == null)
                throw new Exception("Unable to find player's profile");

            return playerProfileLine.Substring(playerProfileLine.IndexOf("=") + 1);
        }

        public static void SaveAsJson(string destinationPath, GameResult gameResult)
        {
            string json = JsonConvert.SerializeObject(gameResult);
            File.WriteAllText(destinationPath, json);
        }

        public static void CreateStructure()
        {
            string replaysWatcherPath = Path.Combine(Constants.SoulstormInstallPath, "ReplaysWatcher");
            if (!Directory.Exists(replaysWatcherPath))
                Directory.CreateDirectory(replaysWatcherPath);

            string unsentFilesPath = Path.Combine(Constants.SoulstormInstallPath, "Playback", "Unsent Ladder Games");
            if (!Directory.Exists(unsentFilesPath))
                Directory.CreateDirectory(unsentFilesPath);
        }

        public static void DeleteIfExists(string path)
        {
            if (File.Exists(path))
                File.Delete(path);
        }

        public static string GetPlayerName()
        {
            string filePath = Path.Combine(Constants.SoulstormInstallPath, "Profiles", FileHelper.GetPlayerProfile(), "name.dat");
            string name = File.ReadAllText(filePath);

            return name;
        }

        public static string GetVersion(string modName)
        {
            string filePath = Path.Combine(Constants.SoulstormInstallPath, $"{modName}.module");

            if (!File.Exists(filePath)) return string.Empty;

            string[] moduleFileLines = File.ReadAllLines(filePath);

            string versionLine = moduleFileLines
                .Where(el => el.StartsWith("ModVersion"))
                .DefaultIfEmpty(string.Empty)
                .First()
                .RemoveWhitespace();

            return string.IsNullOrEmpty(versionLine) ? string.Empty : versionLine.Substring(versionLine.IndexOf("=") + 1);
        }
        
        public static void CopyRelicChunkyFile(string sourceFilePath, string destinationPath, RelicChunkyData relicChunkyData)
        {
            var fileBytes = File.ReadAllBytes(sourceFilePath);

            var beginning = fileBytes
                .Take((int)relicChunkyData.HeaderLengthPosition);
            var beforeDatabaseContent = fileBytes
                .Skip((int)(relicChunkyData.HeaderLengthPosition + 4))
                .Take((int)(relicChunkyData.DatabaseChunkLengthPosition - relicChunkyData.HeaderLengthPosition - 4));
            var databaseContent = fileBytes
                .Skip((int)relicChunkyData.DatabaseChunkLengthPosition + 4)
                .Take(85);
            var end = fileBytes
                .Skip((int)relicChunkyData.ReplayNameValueEndPosition);

            byte[] replayName = Encoding.Unicode.GetBytes(relicChunkyData.MapName);
            byte[] replayNameLength = BitConverter.GetBytes(replayName.Length / 2);

            int headerLength = relicChunkyData.HeaderLength - relicChunkyData.ReplayName.Length * 2 + replayName.Length;
            byte[] headerLengthAsBytes = BitConverter.GetBytes(headerLength);
            int databaseChunkLength = relicChunkyData.DatabaseChunkLength - relicChunkyData.ReplayName.Length * 2 + replayName.Length;
            byte[] databaseChunkLengthAsBytes = BitConverter.GetBytes(databaseChunkLength);

            IEnumerable<byte> alteredData = beginning
                .Concat(headerLengthAsBytes)
                .Concat(beforeDatabaseContent)
                .Concat(databaseChunkLengthAsBytes)
                .Concat(databaseContent)
                .Concat(replayNameLength)
                .Concat(replayName)
                .Concat(end);

            File.WriteAllBytes(destinationPath, alteredData.ToArray());
        }

        public static bool? IsCurrentFilePlayback()
        {
            string filePath = Path.Combine(Constants.SoulstormInstallPath, "warnings.log");
            if (!File.Exists(filePath)) return null;

            List<string> appLines = File.ReadAllLines(filePath)
                .Where(l => l.Contains("APP -- ")).ToList();

            var lastGameEvents = appLines
                .Skip(Math.Max(0, appLines.Count() - 3));

            if (lastGameEvents.Count() != 3
            || !lastGameEvents.Last().Contains("APP -- Game Stop")
            || !lastGameEvents.ElementAt(1).Contains("APP -- Game Start"))
                return null;

            if (lastGameEvents.First().Contains("APP -- Game Playback"))
                return true;

            return false;
        }
    }
}
