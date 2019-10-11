using DoWproReplayWatcher.Logic;
using DoWproReplayWatcher.Logic.Application;
using DoWproReplayWatcher.Logic.Communication;
using DoWproReplayWatcher.Logic.Extensions;
using DoWproReplayWatcher.Logic.Helpers;
using DoWproReplayWatcher.Logic.Lua;
using DoWproReplayWatcher.Logic.Types;
using DoWproReplayWatcher.Lua;
using DoWproReplayWatcher.RelicChunky;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;

namespace DoWproReplayWatcher.ManualTesting
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
            //            new KeyValuePair<string, string>("login", "**************"),
            //            new KeyValuePair<string, string>("password", "*************")
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

            /////////////////////////////////////////////////////////////////////////////////////////
            ///
            FileHelper.CreateStructure();
            DoWproLadderApi.ApiUrl = "https://dowpro.cf/api";//ConfigurationManager.AppSettings["ApiUrl"];
                                                             // Logger logger = new Logger();
                                                             // await MainLogic.CheckPlayback(logger);

            bool? afddf = FileHelper.IsCurrentFilePlayback();

            /////////////////////////////////////////////////////////////////////////////////////////

            // await Encrypt();

            /////////////////////////////////////////////////////////////////////////////////////////

            //string file = "D:\\SteamGames\\steamapps\\common\\Dawn of War Soulstorm\\Playback\\Pro_2p_quests_triumph_08152019-163638.rec";
            //var rep = ReplayParser.Fetch(file);

            //var fileBytes = File.ReadAllBytes(file);

            //var beginning = fileBytes.Take((int)rep.HeaderLengthPosition);
            //var beforeDatabaseContent = fileBytes.Skip((int)(rep.HeaderLengthPosition + 4)).Take((int)(rep.DatabaseChunkLengthPosition - rep.HeaderLengthPosition - 4));
            //var databaseContent = fileBytes.Skip((int)rep.DatabaseChunkLengthPosition + 4).Take(85);
            //var end = fileBytes.Skip((int)rep.ReplayNameValueEndPosition);

            //byte[] replayName = Encoding.Unicode.GetBytes(rep.MapName);
            ////new UnicodeEncoding().GetBytes("supertanker");
            //byte[] replayNameLength = BitConverter.GetBytes(replayName.Length/2);

            //int headerLength = rep.HeaderLength - rep.ReplayName.Length * 2 + replayName.Length;
            //byte[] headerLengthAsBytes = BitConverter.GetBytes(headerLength);
            //int databaseChunkLength = rep.DatabaseChunkLength - rep.ReplayName.Length * 2 + replayName.Length;
            //byte[] databaseChunkLengthAsBytes = BitConverter.GetBytes(databaseChunkLength);

            //IEnumerable<byte> alteredData = beginning
            //    .Concat(headerLengthAsBytes)
            //    .Concat(beforeDatabaseContent)
            //    .Concat(databaseChunkLengthAsBytes)
            //    .Concat(databaseContent)
            //    .Concat(replayNameLength)
            //    .Concat(replayName)
            //    .Concat(end);

            //File.WriteAllBytes("D:\\SteamGames\\steamapps\\common\\Dawn of War Soulstorm\\Playback\\yolo23.rec", alteredData.ToArray());

            /////////////////////////////////////////////////////////////////////////////////////////

            //RelicChunkyData replayData = ReplayParser.Fetch(Constants.TempRecPath);
            //string savedFileName = $"{replayData.MapName}_{DateTime.UtcNow.ToString("MMddyyyy-HHmmss")}.rec";
            //string savedFilePath = Path.Combine(Constants.PlayBackPath, savedFileName);
            //FileHelper.CopyRelicChunkyFile(Constants.TempRecPath, savedFilePath, replayData);

            //var i = FileHelper.IsAlreadySaved(Constants.PlayBackPath, savedFileName);


            int a = 0;
        }

        private static void Encrypt()
        {
            string resultFilePath = Path.Combine(
                   Constants.SoulstormInstallPath,
                   "Profiles",
                   FileHelper.GetPlayerProfile(),
                   "testStats.Lua");

            GameResult result = LuaParser.ParseGameResult(resultFilePath);

            string jsonPath = Path.Combine(Constants.SoulstormInstallPath, "ReplaysWatcher", "result.json");
            FileHelper.SaveAsJson(jsonPath, result);

            string archivePath = Path.Combine(Constants.SoulstormInstallPath, "ReplaysWatcher", "arc.zip");

            FileHelper.DeleteIfExists(archivePath);

            ArchiveHelper.CreateZipFile(archivePath, new List<string>()
            {
                "D:\\SteamGames\\steamapps\\common\\Dawn of War Soulstorm\\Playback\\2P_OUTER_REACHES_07132019-191136.rec",
                jsonPath
            });

            AesManaged aes = new AesManaged();
            // aes.GenerateKey();
            //aes.GenerateIV();
            //Console.WriteLine($"new byte[] {{{string.Join(",", aes.Key)}}}");
            //Console.WriteLine($"new byte[] {{{string.Join(",", aes.IV)}}}");
            CryptoHelper.EncryptFile(archivePath, $"{archivePath}.enc");

            //   CryptoHelper.DecryptFile($"{archivePath}.enc", key);

            //File.WriteAllBytes($"{archivePath}.enc", d);
            int a = 0;
        }

 
    }
}