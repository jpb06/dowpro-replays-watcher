using DoWproReplayWatcher.Logic;
using DoWproReplayWatcher.Logic.Communication;
using DoWproReplayWatcher.Logic.Helpers;
using DoWproReplayWatcher.Logic.Types;
using DoWproReplayWatcher.Lua;
using DoWproReplayWatcher.RelicChunky;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.ServiceProcess;
using System.Threading;

namespace DoWproReplayWatcher.Service
{
    public partial class DoWproWatcherService : ServiceBase
    {
        private Thread loopThread;

        public DoWproWatcherService()
        {
            InitializeComponent();

            this.eventLog = new EventLog();
            if (!EventLog.SourceExists("DoWproWatcherService"))
                EventLog.CreateEventSource("DoWproWatcherService", "Application");

            this.eventLog.Source = "DoWproWatcherService";
            this.eventLog.Log = "Application";

            this.loopThread = null;
        }

        protected override void OnStart(string[] args)
        {
            FileHelper.CreateStructure();

            this.loopThread = new Thread(CheckPlayback);
            this.loopThread.Start();
        }

        protected override void OnStop()
        {
            this.loopThread.Abort();
        }

        private async void CheckPlayback()
        {
            while (true)
            {
                // No temp.rec
                if (!File.Exists(Constants.TempRecPath))
                {
                    Thread.Sleep(30000);
                    continue;
                }

                // File is in use
                bool isOpened = FileHelper.IsOpened(Constants.TempRecPath);
                if (isOpened)
                {
                    Thread.Sleep(30000);
                    continue;
                }

                // Already saved
                if (FileHelper.IsAlreadySaved(Constants.PlayBackPath))
                {
                    Thread.Sleep(30000);
                    continue;
                }

                string savedFileName = $"{ReplayParser.GetMapName(Constants.TempRecPath)}_{DateTime.UtcNow.ToString("MMddyyyy-HHmmss")}.rec";
                string savedFilePath = Path.Combine(Constants.PlayBackPath, savedFileName);
                File.Copy(Constants.TempRecPath, savedFilePath);

                string resultFilePath = Path.Combine(
                   Constants.SoulstormInstallPath,
                   "Profiles",
                   FileHelper.GetPlayerProfile(),
                   "testStats.Lua");

                GameResult result = LuaParser.ParseGameResult(resultFilePath);

                // verify if we should send

                // only winners send results
                // only 1vs1 are sent
                // only player vs player
                string playerName = FileHelper.GetPlayerName();
                if (result != null
                && result.TeamsCount == 2
                && result.PlayersCount == 2
                && result.Players.Count == 2
                && !string.IsNullOrEmpty(result.WinCondition)
                && result.Players
                    .Where(el => el.Name == playerName)
                    .Where(el => el.IsHuman)
                    .Where(el => el.IsAmongWinners)
                    .Any()
                && result.Players.All(el => el.IsHuman))
                {
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

                    byte[] archiveData = File.ReadAllBytes(archivePath);
                    // Send
                    var r = await DoWproLadderApi.SendResult(archiveData);
                    this.eventLog.WriteEntry($"{DateTime.Now} - {savedFileName} : {r}");

                    FileHelper.DeleteIfExists(archivePath);
                    FileHelper.DeleteIfExists(jsonPath);
                }
                else
                {
                    this.eventLog.WriteEntry($"{DateTime.Now} - {savedFileName} : Invalid file");
                    Thread.Sleep(30000);
                }
            }
        }
    }
}