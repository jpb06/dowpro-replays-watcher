using DoWproReplayWatcher.Logic;
using DoWproReplayWatcher.Logic.Communication;
using DoWproReplayWatcher.Logic.Extensions;
using DoWproReplayWatcher.Logic.Helpers;
using DoWproReplayWatcher.Logic.Lua;
using DoWproReplayWatcher.Logic.Types;
using DoWproReplayWatcher.Lua;
using DoWproReplayWatcher.RelicChunky;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;

namespace tests
{
    class Program
    {
        static async Task Main(string[] args)
        {
            //string savedFileName = $"{ReplayParser.GetMapName(Constants.TempRecPath)}_{DateTime.UtcNow.ToString("MMddyyyy-HHmmss")}.rec";
            //string savedFilePath = Path.Combine(Constants.PlayBackPath, savedFileName);
            //File.Copy(Constants.TempRecPath, savedFilePath);

            //GameResult result = ResultFileHelper.Parse();
            //string jsonPath = Path.Combine(Constants.SoulstormInstallPath, "ReplaysWatcher", "result.json");
            //FileHelper.SaveAsJson(jsonPath, result);

            //string archivePath = Path.Combine(Constants.SoulstormInstallPath, "ReplaysWatcher", "arc.zip");

            //FileHelper.DeleteIfExists(archivePath);

            //ArchiveHelper.CreateZipFile(archivePath, new List<string>()
            //{
            //    savedFilePath,
            //    jsonPath
            //});

            //byte[] archiveData = File.ReadAllBytes(archivePath);

            //Task t = Task.Run(async () =>
            //{
            //    const string url = "http://localhost:3001/api";
            //    //"https://dowpro.cf";

            //    using (HttpClient client = new HttpClient())
            //    {
            //        var content = new MultipartFormDataContent();
            //        var archiveContent = new StreamContent(new MemoryStream(archiveData));
            //        archiveContent.Headers.ContentType = MediaTypeHeaderValue.Parse("application/zip");

            //        var c = new FormUrlEncodedContent(new[]
            //        {
            //            new KeyValuePair<string, string>("login", "dowpro-replays-watcher-api-usr"),
            //            new KeyValuePair<string, string>("password", "6XHB9JXcr1511oV")
            //        });
            //        var res = await client.PostAsync($"{url}/login", c);
            //        var co = await res.Content.ReadAsStringAsync();

            //        AuthResult r = JsonConvert.DeserializeObject<AuthResult>(co);

            //        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", r.Token);
            //        content.Add(archiveContent, "arc", "arc.zip");
            //        var response = await client.PostAsync($"{url}/sendResult", content);
            //        var co2 = await response.Content.ReadAsStringAsync();

            //        int a = 0;
            //    }
            //});
            //t.Wait();

            ///////////////////////////////////////////////////////////////////////////////////////////

            //string path = Path.Combine(
            //    Constants.SoulstormInstallPath,
            //    "Profiles",
            //    FileHelper.GetPlayerProfile(),
            //    "testStats.Lua");

            //string fileContent = File.ReadAllText(path).ClearLua();
            //var root = LuaParser.Parse(fileContent);

            //var stats = root.GetValue<LuaObject>("GSGameStats");
            //int duration = stats.GetValue<int>("Duration");
            //var p1 = stats.GetValue<LuaObject>("player_1");

            //bool r = stats.Is<string>("Scenario");

            //var res = root.AsGameResult();

            /////////////////////////////////////////////////////////////////////////////////////////

            //System.Timers.Timer aTimer = new System.Timers.Timer();
            //aTimer.Elapsed += new ElapsedEventHandler(SoulstormProcessWatching);
            //aTimer.Interval = 60000;
            //aTimer.Enabled = true;

            FileHelper.CreateStructure();
            await CheckPlayback();
        }

        private static async Task CheckPlayback()
        {
            //bool IsSaveOperationRequired = false;

            while (true)
            {
                //while (ProcessHelper.IsProcessRunning("Soulstorm") || IsSaveOperationRequired)
                //{
                if (!File.Exists(Constants.TempRecPath))
                {
                    //Console.WriteLine("No temp.rec");
                    Thread.Sleep(5000);
                    //IsSaveOperationRequired = false;
                    continue;
                }

                bool isOpened = FileHelper.IsOpened(Constants.TempRecPath);
                if (isOpened)
                {
                    //Console.WriteLine("File is in use");
                    //IsSaveOperationRequired = true;
                    Thread.Sleep(5000);
                    continue;
                }

                if (FileHelper.IsAlreadySaved(Constants.PlayBackPath))
                {
                    //Console.WriteLine("File is already saved");
                    //IsSaveOperationRequired = false;
                    Thread.Sleep(5000);
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
                && result.TeamsCount == 2)
                //&& result.PlayersCount == 2
                //&& result.Players.Count == 2
                //&& !string.IsNullOrEmpty(result.WinCondition)
                //&& result.Players
                //    .Where(el => el.Name == playerName)
                //    .Where(el => el.IsHuman)
                //    .Where(el => el.IsAmongWinners)
                //    .Any()
                //&& result.Players.All(el => el.IsHuman))
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
                    Console.WriteLine(r);

                    FileHelper.DeleteIfExists(archivePath);
                    FileHelper.DeleteIfExists(jsonPath);
                    //IsSaveOperationRequired = false;
                }
                else
                {
                    Console.WriteLine("Invalid file");
                    //IsSaveOperationRequired = false;
                    Thread.Sleep(5000);
                }
            }

            //Console.WriteLine.WriteEntry("Soulstorm is stopped");
            //Thread.Sleep(60000);
        }
    }
}