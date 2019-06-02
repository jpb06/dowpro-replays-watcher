using LuaInterface;
using Microsoft.Win32;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using tests.Helpers;
using tests.Types;

namespace tests
{
    class Program
    {
        private static bool isSoulstormRunning = false;
        private static string soulstormInstallPath = RegistryHelper.GetSoulstormInstallPath();

        private static string playBackPath = Path.Combine(soulstormInstallPath, "Playback");
        private static string tempRecPath = Path.Combine(soulstormInstallPath, "Playback", "temp.rec");

        static void Main(string[] args)
        {
            FileHelper.CreateStructure(soulstormInstallPath);

            while (true)
            {
                isSoulstormRunning = ProcessHelper.IsProcessRunning("Soulstorm");
                CheckPlayback();
                Thread.Sleep(10000);
            }

        }

        private static void CheckPlayback()
        {
            while (isSoulstormRunning)
            {
                if (File.Exists(tempRecPath))
                {
                    bool isOpened = FileHelper.IsOpened(tempRecPath);
                    if (!isOpened)
                    {
                        if (!FileHelper.IsAlreadySaved(playBackPath))
                        {
                            string savedFileName = $"{ReplayParser.GetMapName(tempRecPath)}_{DateTime.UtcNow.ToString("MMddyyyy-HHmmss")}.rec";
                            string savedFilePath = Path.Combine(playBackPath, savedFileName);
                            File.Copy(tempRecPath, savedFilePath);

                            GameResult result = ResultFileHelper.Parse(soulstormInstallPath);
                            string jsonPath = Path.Combine(soulstormInstallPath, "ReplaysWatcher", "result.json");
                            FileHelper.SaveAsJson(jsonPath, result);

                            string archivePath = Path.Combine(soulstormInstallPath, "ReplaysWatcher", "arc.zip");

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
            }
        }
    }
}
