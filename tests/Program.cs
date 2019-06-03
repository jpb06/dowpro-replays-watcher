using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using tests.Helpers;
using tests.Types;

namespace tests
{
    class Program
    {
        private static bool isSoulstormRunning = false;

        static void Main(string[] args)
        {
            FileHelper.CreateStructure();

            while (true)
            {
                isSoulstormRunning = ProcessHelper.IsProcessRunning("Soulstorm");
                CheckPlayback();
                Thread.Sleep(60000);
            }

        }

        private static void CheckPlayback()
        {
            while (isSoulstormRunning)
            {
                if (File.Exists(Constants.TempRecPath))
                {
                    bool isOpened = FileHelper.IsOpened(Constants.TempRecPath);
                    if (!isOpened)
                    {
                        if (!FileHelper.IsAlreadySaved(Constants.PlayBackPath))
                        {
                            string savedFileName = $"{ReplayParser.GetMapName(Constants.TempRecPath)}_{DateTime.UtcNow.ToString("MMddyyyy-HHmmss")}.rec";
                            string savedFilePath = Path.Combine(Constants.PlayBackPath, savedFileName);
                            File.Copy(Constants.TempRecPath, savedFilePath);

                            GameResult result = ResultFileHelper.Parse();

                            // verify if we should send
                            string playerName = FileHelper.GetPlayerName();
                            if (result.Players
                                .Where(el => el.IsHuman)
                                .Where(el => el.IsAmongWinners)
                                .Where(el => el.Name == playerName).Any())
                            {
                                // only winners send results

                                // should prob check if result and replay match

                                string jsonPath = Path.Combine(Constants.SoulstormInstallPath, "ReplaysWatcher", "result.json");
                                FileHelper.SaveAsJson(jsonPath, result);

                                string archivePath = Path.Combine(Constants.SoulstormInstallPath, "ReplaysWatcher", "arc.zip");

                                FileHelper.DeleteIfExists(archivePath);

                                ArchiveHelper.CreateZipFile(archivePath, new List<string>()
                                {
                                    savedFilePath,
                                    jsonPath
                                });

                                // Send

                                FileHelper.DeleteIfExists(archivePath);
                                FileHelper.DeleteIfExists(jsonPath);
                            }
                        }
                        else
                        {
                            Thread.Sleep(10000);
                        }
                    }
                    else
                    {
                        Thread.Sleep(30000);
                    }
                }
            }
        }
    }
}
