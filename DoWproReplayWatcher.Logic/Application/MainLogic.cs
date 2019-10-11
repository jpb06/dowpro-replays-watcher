using DoWproReplayWatcher.Logic.Communication;
using DoWproReplayWatcher.Logic.Helpers;
using DoWproReplayWatcher.Logic.Types;
using DoWproReplayWatcher.Lua;
using DoWproReplayWatcher.RelicChunky;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DoWproReplayWatcher.Logic.Application
{
    public class MainLogic
    {
        public static async Task CheckPlayback(ILogger logger)
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

                bool? isCurrentFileDueToPlayback = FileHelper.IsCurrentFilePlayback();
                if(isCurrentFileDueToPlayback == null)
                {
                    logger.WriteEntry($"{ DateTime.Now} - Unable to detect if the file is playback");
                }
                else if (isCurrentFileDueToPlayback == true)
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

                string playerName = FileHelper.GetPlayerName();

                // verify if we should send

                // only winners send results
                // only 1vs1 are sent
                // only player vs player
                // 3.30 minutes duration minimum
                string discardedGameReason = string.Empty;
                if (result == null)
                    discardedGameReason = "Lua game result was null.";
                else
                {
                    if(result.TeamsCount != 2 || result.PlayersCount != 2 || result.Players.Count != 2)
                        discardedGameReason = "Game was not a 1vs1.";
                    if(string.IsNullOrEmpty(result.WinCondition))
                        discardedGameReason = "Game had no win condition.";
                    if(!result.Players
                        .Where(el => el.Name == playerName)
                        .Where(el => el.IsHuman)
                        .Where(el => el.IsAmongWinners)
                        .Any())
                        discardedGameReason = "Only winners send game results.";
                    if(!result.Players.All(el => el.IsHuman))
                        discardedGameReason = "Game was vs A.I.";
                    if(result.Duration < 210)
                        discardedGameReason = "Game duration was below 3.30 minutes.";
                }
                
                if (string.IsNullOrEmpty(discardedGameReason))
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

                    logger.WriteEntry($"{DateTime.Now} - {savedFileName} : {uploadResult}");

                    FileHelper.DeleteIfExists(archivePath);
                    FileHelper.DeleteIfExists(jsonPath);
                    // FileHelper.DeleteIfExists(resultFilePath);
                }
                else
                {
                    logger.WriteEntry($"{DateTime.Now} - {savedFileName} : {discardedGameReason}");
                    Thread.Sleep(30000);
                }
            }
        }
    }
}
