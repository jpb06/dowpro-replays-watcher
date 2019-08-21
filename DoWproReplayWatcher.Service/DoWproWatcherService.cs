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
using System.Configuration;

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
            DoWproLadderApi.ApiUrl = ConfigurationManager.AppSettings["ApiUrl"];
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

                // Couldn't parse rec file
                RelicChunkyData replayData = ReplayParser.Fetch(Constants.TempRecPath);
                if (replayData == null)
                {
                    Thread.Sleep(60000);
                    continue;
                }

                // Not a dowpro game
                if (replayData.ModName != "dowpro" && replayData.ModName != "dowprotest")
                {
                    Thread.Sleep(60000);
                    continue;
                }

                string modVersion = FileHelper.GetVersion(replayData.ModName);
                string savedFileName = $"{replayData.MapName}_{DateTime.UtcNow.ToString("MMddyyyy-HHmmss")}.rec";
                string savedFilePath = Path.Combine(Constants.PlayBackPath, savedFileName);
                FileHelper.CopyRelicChunkyFile(Constants.TempRecPath, savedFilePath, replayData);

                // Already saved
                if (FileHelper.IsAlreadySaved(Constants.PlayBackPath, savedFileName))
                {
                    File.Delete(savedFilePath);
                    Thread.Sleep(30000);
                    continue;
                }

                string resultFilePath = Path.Combine(
                   Constants.SoulstormInstallPath,
                   "Profiles",
                   FileHelper.GetPlayerProfile(),
                   "testStats.Lua");

                GameResult result = LuaParser.ParseGameResult(resultFilePath);
                result.ModVersion = modVersion;
                result.ModName = replayData.ModName;

                // verify if we should send

                // only winners send results
                // only 1vs1 are sent
                // only player vs player
                // 3.30 minutes duration minimum
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
                && result.Players.All(el => el.IsHuman)
                && result.Duration >= 210) 
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

                    string unsentLadderFileName = $"{replayData.MapName}_{DateTime.UtcNow.ToString("MMddyyyy-HHmmss")}.ladder";
                    string unsentLadderFilePath = Path.Combine(Constants.SoulstormInstallPath, "Playback", "Unsent Ladder Games", unsentLadderFileName);
                    CryptoHelper.EncryptFile(
                        archivePath,
                        unsentLadderFilePath);

                    byte[] archiveData = File.ReadAllBytes(archivePath);
                    // Send
                    string uploadResult = await DoWproLadderApi.SendResult(archiveData);

                    if (uploadResult == "Added"
                    || uploadResult.StartsWith("Unable to unzip ")
                    || uploadResult.StartsWith("Unable to locate replay file for ")
                    || uploadResult.StartsWith("Unable to compute file hash for")
                    || uploadResult.EndsWith("Already exists in db store")
                    || uploadResult.StartsWith("Unable to parse game result for")
                    || uploadResult.StartsWith("Results did not match for")
                    || uploadResult.StartsWith("Invalid players count for")
                    )
                        File.Delete(unsentLadderFilePath);

                    this.eventLog.WriteEntry($"{DateTime.Now} - {savedFileName} : {uploadResult}");

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