using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using tests.Types;

namespace tests.Helpers
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

        public static bool IsAlreadySaved(string directoryPath)
        {
            if (Directory.Exists(directoryPath))
            {
                DirectoryInfo directory = new DirectoryInfo(directoryPath);

                FileInfo lastWrittenFile = directory
                    .GetFiles("*.rec")
                    .Where(f => f.Name != "temp.rec")
                    .OrderByDescending(f => f.LastWriteTime)
                    .FirstOrDefault();

                if (lastWrittenFile == null) return false;

                FileInfo tempRec = new FileInfo(Path.Combine(directoryPath, "temp.rec"));

                if (lastWrittenFile.Length != tempRec.Length)
                    return false;

                try
                {
                    using (FileStream fs1 = new FileStream(lastWrittenFile.FullName, FileMode.Open,
                                  FileAccess.Read, FileShare.ReadWrite))
                    using (FileStream fs2 = new FileStream(tempRec.FullName, FileMode.Open,
                                  FileAccess.Read, FileShare.ReadWrite))
                    {
                        for (int i = 0; i < tempRec.Length; i++)
                        {
                            if (fs1.ReadByte() != fs2.ReadByte())
                                return false;
                        }

                        fs1.Close();
                        fs2.Close();
                    }
                }
                catch (Exception)
                {
                    // in case files are already opened somehow
                    return true;
                }

                return true;
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
            string path = Path.Combine(Constants.SoulstormInstallPath, "ReplaysWatcher");
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
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
    }
}
