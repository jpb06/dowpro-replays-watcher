using LuaInterface;
using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using tests.Types;

namespace tests.Helpers
{
    public static class ResultFileHelper
    {
        public static GameResult Parse(string soulstormInstallPath)
        {
            Lua lua = new Lua();

            object[] luaResult = lua.DoFile(Path.Combine(soulstormInstallPath, "Profiles", FileHelper.GetPlayerProfile(soulstormInstallPath), "testStats.Lua"));

            LuaTable gsGameStatsTable = lua.GetTable("GSGameStats");

            if (gsGameStatsTable == null)
                throw new Exception("GSGameStats doesn't exist");

            GameResult gameResult = new GameResult();

            foreach (DictionaryEntry mainMember in gsGameStatsTable)
            {
                string key = mainMember.Key.ToString();

                if (key == "Players")
                    gameResult.PlayersCount = Int32.Parse(mainMember.Value.ToString());
                else if (key == "WinBy")
                    gameResult.WinCondition = mainMember.Value.ToString();
                else if(key == "Teams")
                    gameResult.TeamsCount = Int32.Parse(mainMember.Value.ToString());
                else if (key == "Duration")
                    gameResult.Duration = Int32.Parse(mainMember.Value.ToString());
                else if (key == "Scenario")
                    gameResult.MapName = mainMember.Value.ToString();
                else if (key.StartsWith("player_"))
                {
                    GamePlayer player = new GamePlayer();

                    LuaTable playerTable = lua.GetTable($"GSGameStats.{key.ToString()}");
                    foreach(DictionaryEntry playerMember in playerTable)
                    {
                        string playerKey = playerMember.Key.ToString();

                        if (playerKey == "PRace")
                            player.Race = playerMember.Value.ToString();
                        else if (playerKey == "PHuman")
                            player.IsHuman = playerMember.Value.ToString() == "1";
                        else if (playerKey == "PFnlState")
                        {
                            int state = Int32.Parse(playerMember.Value.ToString());
                            player.IsAmongWinners = state == 5;
                        }
                        else if (playerKey == "PTeam")
                            player.Team = Int32.Parse(playerMember.Value.ToString());
                        else if (playerKey == "PName")
                            player.Name = playerMember.Value.ToString();
                        else if (playerKey == "PTtlSc")
                            player.PTtlSc = Int32.Parse(playerMember.Value.ToString());
                    }

                    gameResult.Players.Add(player);
                }
            }

            return gameResult;
        }
    }
}
